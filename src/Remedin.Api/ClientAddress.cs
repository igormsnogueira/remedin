using System.Net;
using System.Net.Sockets;

namespace Remedin.Api;

/// <summary>
/// O endereço IP de quem fez a requisição.
///
/// Em produção a API fica atrás do proxy do Render, e o endereço da conexão é
/// o do proxy. O de quem acessa vem nos cabeçalhos que ele acrescenta.
///
/// Os cabeçalhos podem ser forjados por quem chama. Aqui isso não é risco: o
/// único efeito é a pessoa escolher o próprio estado sugerido, o que ela
/// também pode fazer no seletor.
/// </summary>
internal static class ClientAddress
{
    /// <summary>Do mais confiável para o menos, como os proxies costumam preencher.</summary>
    private static readonly string[] Headers = ["CF-Connecting-IP", "True-Client-IP", "X-Forwarded-For"];

    public static IPAddress? From(HttpContext context)
    {
        foreach (var header in Headers)
        {
            var value = context.Request.Headers[header].ToString();

            // X-Forwarded-For vem como "cliente, proxy1, proxy2": o primeiro
            // é quem abriu a conexão.
            var first = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (first is not null && IPAddress.TryParse(first, out var parsed))
            {
                return parsed;
            }
        }

        return context.Connection.RemoteIpAddress;
    }

    /// <summary>
    /// Endereço de rede interna, como o da máquina de desenvolvimento, não
    /// aparece na base. Responder logo evita uma consulta inútil.
    /// </summary>
    public static bool IsPrivate(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
        {
            return true;
        }

        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6UniqueLocal;
        }

        var bytes = address.GetAddressBytes();

        return bytes[0] == 10
            || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            || (bytes[0] == 192 && bytes[1] == 168)
            || (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127);
    }
}
