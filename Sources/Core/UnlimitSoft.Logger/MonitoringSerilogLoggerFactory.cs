using Microsoft.Extensions.Logging;
using Serilog;
using System;

namespace UnlimitSoft.Logger;


/// <summary>
/// Create a serilog factory to wrapper other factory and with the ability to update the internal logger create with this factory to use the updated factory.
/// </summary>
public sealed class MonitoringSerilogLoggerFactory : ILoggerFactory
{
    private int _version;
    private ILoggerFactory _inner;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    public MonitoringSerilogLoggerFactory(Serilog.ILogger logger)
    {
        _inner = LoggerFactory.Create(builder => builder.AddSerilog(logger));
    }

    /// <summary>
    /// Version of the factory
    /// </summary>
    internal int Version => _version;
    /// <summary>
    /// Internal factory
    /// </summary>
    internal ILoggerFactory Inner => _inner;

    /// <inheritdoc />
    public void Dispose() => _inner.Dispose();
    /// <inheritdoc />
    public void AddProvider(ILoggerProvider provider) => _inner.AddProvider(provider);
    /// <inheritdoc />
    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName) => new Logger(this, categoryName);

    /// <summary>
    /// Update the internal serilog logger for a new one
    /// </summary>
    /// <param name="logger"></param>
    public void Update(Serilog.ILogger logger)
    {
        var aux = _inner;

        _inner = LoggerFactory.Create(builder => builder.AddSerilog(logger));
        _version++;

        aux?.Dispose();
    }

    #region Nested Classes
    private sealed class Logger : Microsoft.Extensions.Logging.ILogger
    {
        private int _version;
        private Microsoft.Extensions.Logging.ILogger _logger;
        private readonly MonitoringSerilogLoggerFactory _owner;
        private readonly string _categoryName;

        public Logger(MonitoringSerilogLoggerFactory owner, string categoryName)
        {
            _owner = owner;
            _categoryName = categoryName;
            _version = owner.Version;
            _logger = _owner.Inner.CreateLogger(categoryName);
        }

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            CheckRecreate();
            return _logger.BeginScope(state);
        }
        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            CheckRecreate();
            return _logger.IsEnabled(logLevel);
        }
        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            CheckRecreate();
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }

        #region Private Methods
        private void CheckRecreate()
        {
            if (_version == _owner.Version)
                return;

            _version = _owner.Version;
            _logger = _owner.Inner.CreateLogger(_categoryName);
        }
        #endregion
    }
    #endregion
}