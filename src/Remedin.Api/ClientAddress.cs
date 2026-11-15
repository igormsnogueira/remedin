using System.Net;
using System.Net.Sockets;

namespace Remedin.Api;

internal static class ClientAddress
{
    // Atrás do proxy do Render, o IP da conexão é o do proxy. Cabeçalho forjado só muda o estado sugerido.
    private static readonly string[] Headers = ["CF-Connecting-IP", "True-Client-IP", "X-Forwarded-For"];

    public static IPAddress? From(HttpContext context)
    {
        foreach (var header in Headers)
        {
            var value = context.Request.Headers[header].ToString();

            var first = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (first is not null && IPAddress.TryParse(first, out var parsed))
            {
                return parsed;
            }
        }

        return context.Connection.RemoteIpAddress;
    }

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
