using System.Net;
using System.Numerics;

namespace Remedin.Infrastructure.Ingestion.DbIp;

internal static class IpMath
{
    public static bool IsNext(IPAddress end, IPAddress start)
    {
        if (end.AddressFamily != start.AddressFamily)
        {
            return false;
        }

        return ToNumber(end) + 1 == ToNumber(start);
    }

    private static BigInteger ToNumber(IPAddress address) =>
        new(address.GetAddressBytes(), isUnsigned: true, isBigEndian: true);
}
