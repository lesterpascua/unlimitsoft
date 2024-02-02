using FluentAssertions;
using System.Collections.Generic;
using System.Net;
using UnlimitSoft.Network;
using Xunit;

namespace UnlimitSoft.Tests.UnlimitSoft.Network;


public sealed class IPNetworkExtensionsTests
{
    [Theory]
    [InlineData("10.20.0.0/16", 10, "10.21.0.0/16")]
    [InlineData("9.255.0.0/17", 20, "9.255.128.0/17")]
    public void TestPreviousAndNextNetwork(string address, int count, string first)
    {
        var list = new List<IPNetwork2>();
        var network = IPNetwork2.Parse(address);

        list.Add(network);
        for (int i = 0; i < count; i++)
        {
            network = network.GetNextNetwork();
            list.Add(network);
            if (i == 0)
                network.Should().Be(IPNetwork2.Parse(first));
        }

        for (int i = count - 1; i >= 0; i--)
        {
            network = network.GetPreviousNetwork();
            list[i].Should().Be(network);
        }
    }
}
