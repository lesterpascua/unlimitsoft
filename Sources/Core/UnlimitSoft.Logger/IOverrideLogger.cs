using Serilog;

namespace UnlimitSoft.Logger;


/// <summary>
/// Use interface to override the logger configuration setter in the application starting point
/// </summary>
public interface IOverrideLogger
{
    /// <summary>
    /// Configure the loggger
    /// </summary>
    /// <param name="loggerConfiguration"></param>
    LoggerConfiguration Configure(LoggerConfiguration loggerConfiguration);
}