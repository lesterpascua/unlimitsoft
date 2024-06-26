﻿using System;
using System.Net;

namespace UnlimitSoft.Network;


/// <summary>
/// Extension 
/// </summary>
public static class IPNetworkExtensions
{
    /// <summary>
    /// Get the previous network from the current. 
    /// </summary>
    /// <param name="this"></param>
    public static IPNetwork2 GetNextNetwork(this IPNetwork2 @this)
    {
        var address = @this.Network.GetAddressBytes();
        Array.Reverse(address);
        var addressNumber = BitConverter.ToUInt32(address, 0);

        var netmask = @this.Netmask.GetAddressBytes();
        Array.Reverse(netmask);
        var netmaskNumber = BitConverter.ToUInt32(netmask, 0);

        var resultNumber = ((addressNumber | ~netmaskNumber) + 1) & netmaskNumber;
        var result = BitConverter.GetBytes(resultNumber);
        Array.Reverse(result);

        var ip = new IPAddress(result);
        return new IPNetwork2(ip, @this.Cidr);
    }
    /// <summary>
    /// Get the previous network from the current. 
    /// </summary>
    /// <param name="this"></param>
    public static IPNetwork2 GetPreviousNetwork(this IPNetwork2 @this)
    {
        var address = @this.Network.GetAddressBytes();
        Array.Reverse(address);
        var addressNumber = BitConverter.ToUInt32(address, 0);

        var netmask = @this.Netmask.GetAddressBytes();
        Array.Reverse(netmask);
        var netmaskNumber = BitConverter.ToUInt32(netmask, 0);

        var resultNumber = ((addressNumber & netmaskNumber) - 1) & netmaskNumber;
        var result = BitConverter.GetBytes(resultNumber);
        Array.Reverse(result);

        var ip = new IPAddress(result);
        return new IPNetwork2(ip, @this.Cidr);
    }
}