using FluentAssertions;
using Microsoft.Extensions.Logging;
using SoftUnlimit.Logger.Extensions;
using System;
using Xunit;

namespace SoftUnlimit.Tests.SoftUnlimit.Logger;

public class LoggerMock : ILogger
{
    private readonly LogLevel _logLevel;

    public LoggerMock(LogLevel logLevel)
    {
        _logLevel = logLevel;
    }

    public string Message { get; set; }
    public int CallLog { get; set; } = 0;
    public LogLevel LogLevel { get; set; }
    public int CallLogLevel { get; set; } = 0;

    public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();

    public bool IsEnabled(LogLevel logLevel)
    {
        CallLogLevel++;
        LogLevel = logLevel;
        return _logLevel <= logLevel;
    }
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        CallLog++;
        Message = formatter(state, exception);
    }
}

public class ILoggerExtensionsTest
{
    private readonly LoggerMock _logger;

    public ILoggerExtensionsTest()
    {
        _logger = new LoggerMock(LogLevel.Information);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData("d1", "other")]
    public void CallLogMethod_WithInfoLevelAndTwoParameters_ShouldPrintProperMessage(object arg0, object arg1)
    {
        // Act
        _logger.LogInformation("Test: {Id} {Value}", arg0: arg0, arg1: arg1);

        // Assert
        _logger.CallLogLevel.Should().Be(1);
        _logger.CallLog.Should().Be(1);

        _logger.LogLevel.Should().Be(LogLevel.Information);
        _logger.Message.Should().Be($"Test: {arg0} {arg1}");
    }
    [Theory]
    [InlineData(1, 2)]
    [InlineData("d1", "other")]
    public void CallLogMethod_WithDebugLevelAndTwoParameters_ShouldSkipMessage(object arg0, object arg1)
    {
        // Act
        _logger.LogDebug("Test: {Id} {Value}", arg0: arg0, arg1: arg1);

        // Assert
        _logger.CallLogLevel.Should().Be(1);
        _logger.CallLog.Should().Be(0);

        _logger.LogLevel.Should().Be(LogLevel.Debug);
        _logger.Message.Should().BeNull();
    }
}