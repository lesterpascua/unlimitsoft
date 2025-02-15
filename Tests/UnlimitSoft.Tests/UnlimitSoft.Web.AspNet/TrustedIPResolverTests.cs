using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Net;
using UnlimitSoft.Web.AspNet;
using Xunit;

namespace UnlimitSoft.Tests.UnlimitSoft.Web.AspNet;



public sealed class TrustedIPResolverTests
{
    private const string UntrustedIP = "192.2.3.4";
    private const string ConnectionIP = "234.2.3.4";
    private const string HeaderXForwardedForIP = "234.2.3.1";

    private readonly HttpContext _context;
    private readonly ConnectionInfo _connection;

    public TrustedIPResolverTests()
    {
        _context = Substitute.For<HttpContext>();
        _connection = Substitute.For<ConnectionInfo>();
        var httpRequest = Substitute.For<HttpRequest>();

        httpRequest.Headers.Returns(_ => new HeaderDictionary { [TrustedIPResolver.HeaderXForwardedFor] = HeaderXForwardedForIP });

        _context.Connection.Returns(_ => _connection);
        _context.Request.Returns(_ => httpRequest);
    }

    [Fact]
    public void GetIPAddress_WhenValidIPsIsNull_ReturnsXForwardedForIP()
    {
        // Arrange
        var resolver = new TrustedIPResolver(null);
        _connection.RemoteIpAddress.Returns(_ => IPAddress.Parse(ConnectionIP));

        // Act
        var result = resolver.GetIPAddress(_context);

        // Assert
        result.Should().Be(HeaderXForwardedForIP);
    }
    [Fact]
    public void GetIPAddress_WhenValidIPsInEmpty_ReturnsConnectionIP()
    {
        // Arrange
        var resolver = new TrustedIPResolver([]);
        _connection.RemoteIpAddress.Returns(_ => IPAddress.Parse(ConnectionIP));

        // Act
        var result = resolver.GetIPAddress(_context);

        // Assert
        result.Should().Be(ConnectionIP);
    }
    [Fact]
    public void GetIPAddress_WhenValidIPsTrustInConnectionIP_ReturnsXForwardedForIP()
    {
        // Arrange
        var resolver = new TrustedIPResolver([ConnectionIP]);
        _connection.RemoteIpAddress.Returns(_ => IPAddress.Parse(ConnectionIP));

        // Act
        var result = resolver.GetIPAddress(_context);

        // Assert
        result.Should().Be(HeaderXForwardedForIP);
    }
    [Fact]
    public void GetIPAddress_WhenValidIPsTrustInConnectionIPButIsDifferentOfTheCurrentConnection_ReturnsConnectionIP()
    {
        // Arrange
        var resolver = new TrustedIPResolver([ConnectionIP]);
        _connection.RemoteIpAddress.Returns(_ => IPAddress.Parse(UntrustedIP));

        // Act
        var result = resolver.GetIPAddress(_context);

        // Assert
        result.Should().Be(UntrustedIP);
    }
}
