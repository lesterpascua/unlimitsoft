using System.Linq;
using System.Net.NetworkInformation;

namespace UnlimitSoft.Security.Cryptography;


/// <summary>
/// Some utility method to generate unique worker.
/// </summary>
public static class Utility
{

    /// <summary>
    /// Get first valid network interface to get the mack address as identifier.
    /// </summary>
    /// <param name="id">Prefered network identifier.</param>
    /// <returns></returns>
    public static NetworkInterface GetNetworkInterface(string? id = null)
    {
        var query = NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(
                nic => nic.OperationalStatus == OperationalStatus.Up &&
                nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                nic.GetPhysicalAddress().GetAddressBytes()?.Length == 6);
        if (!string.IsNullOrEmpty(id))
            return query.First(p => p.Id == id);

        return query.First();
    }
}
