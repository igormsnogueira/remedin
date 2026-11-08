using System.Net;

namespace Remedin.Application.Location;

/// <summary>Uma faixa de endereços IP e o estado em que ela costuma estar.</summary>
public sealed record IpStateRange(IPAddress Start, IPAddress End, string State);

/// <summary>
/// Uma publicação mensal da base de geolocalização, já reduzida às faixas do
/// Brasil.
/// </summary>
/// <param name="Release">Mês da publicação ("2026-11"). Serve de identidade da carga.</param>
/// <param name="RowsRead">Faixas brasileiras lidas, antes de juntar as vizinhas.</param>
/// <param name="Unmapped">Faixas brasileiras com nome de estado que não foi reconhecido.</param>
public sealed record IpStateSnapshot(
    string Release,
    IReadOnlyList<IpStateRange> Ranges,
    int RowsRead,
    int Unmapped);

public interface IIpStateSource
{
    /// <summary>A publicação do mês informado, ou a anterior se ela ainda não saiu.</summary>
    Task<IpStateSnapshot> FetchAsync(DateOnly month, CancellationToken cancellationToken);
}

public interface IIpStateStore
{
    Task ReplaceAllAsync(IReadOnlyList<IpStateRange> ranges, CancellationToken cancellationToken);
}

/// <summary>Estima o estado a partir do endereço IP de quem acessa.</summary>
public interface IStateLocator
{
    /// <returns>A sigla, ou nulo quando o endereço não está em nenhuma faixa conhecida.</returns>
    Task<string?> FindStateAsync(IPAddress address, CancellationToken cancellationToken);
}
