using System.Net;

namespace Remedin.Application.Location;

public sealed record IpStateRange(IPAddress Start, IPAddress End, string State);

public sealed record IpStateSnapshot(
    string Release,
    IReadOnlyList<IpStateRange> Ranges,
    int RowsRead,
    int Unmapped);

public interface IIpStateSource
{
    Task<IpStateSnapshot> FetchAsync(DateOnly month, CancellationToken cancellationToken);
}

public interface IIpStateStore
{
    Task ReplaceAllAsync(IReadOnlyList<IpStateRange> ranges, CancellationToken cancellationToken);
}

public interface IStateLocator
{
    Task<string?> FindStateAsync(IPAddress address, CancellationToken cancellationToken);
}
