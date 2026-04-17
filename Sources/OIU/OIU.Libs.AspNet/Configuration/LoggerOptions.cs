using Microsoft.Extensions.Logging;

namespace OIU.Libs.AspNet.Configuration;


/// <summary>
/// 
/// </summary>
public sealed class LoggerOptions
{
    /// <summary>
    /// Default log level information
    /// </summary>
    public LogLevelOptions? LogLevel { get; set; }

    /// <summary>
    /// Settings for file logger
    /// </summary>
    public FileSettings? File { get; set; }
    /// <summary>
    /// Settings for console logger
    /// </summary>
    public ConsoleSettings? Console { get; set; }
    /// <summary>
    /// Settings for sql server logger
    /// </summary>
    public MSSqlServerSettings? MSSqlServer { get; set; }
    /// <summary>
    /// Settings for application insight
    /// </summary>
    public ApplicationInsightSettings? ApplicationInsight { get; set; }
    /// <summary>
    /// Settings for elastic search
    /// </summary>
    public ElasticSearchOptions? ElasticSearch { get; set; }

    #region Nested Classes
    /// <summary>
    /// 
    /// </summary>
    public sealed class LogLevelOptions : Dictionary<string, LogLevel>
    {
        private const string DEFAULT = "Default";

        /// <summary>
        /// Get default level of logging
        /// </summary>
        public LogLevel Default => TryGetValue(DEFAULT, out var level) ? level : Microsoft.Extensions.Logging.LogLevel.Information;
        /// <summary>
        /// Get all namespaces and the log level (Exclude the default)
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, LogLevel> GetWithoutDefault()
        {
            var aux = new Dictionary<string, LogLevel>(this);
            aux.Remove(DEFAULT);
            return aux;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class ConsoleSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public LogLevelOptions LogLevel { get; set; } = default!;
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class FileSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public LogLevelOptions LogLevel { get; set; } = default!;
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class MSSqlServerSettings
    {
        /// <summary>
        /// Rotate log table automatic, default = false
        /// </summary>
        public bool Rotate { get; set; }
        /// <summary>
        /// Table name
        /// </summary>
        public required string TableName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public required string ConnectionString { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public LogLevelOptions LogLevel { get; set; } = default!;
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class ApplicationInsightSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public LogLevelOptions LogLevel { get; set; } = default!;
        /// <summary>
        /// 
        /// </summary>
        public required string ConnectionString { get; set; }
    }
    /// <summary>
    /// Configuration for elastic search logger
    /// </summary>
    public sealed class ElasticSearchOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public LogLevelOptions LogLevel { get; set; } = default!;
        /// <summary>
        /// Server url
        /// </summary>
        public string Url { get; set; } = default!;
        /// <summary>
        /// Authentication Identifier
        /// </summary>
        public string ApiKey { get; set; } = default!;
    }
    #endregion
}
