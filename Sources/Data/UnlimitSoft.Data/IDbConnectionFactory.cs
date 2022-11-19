using System.Data;

namespace UnlimitSoft.Data;


/// <summary>
/// Represent a connection factory. Using scope to inject this interface.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Get second for command execution time out
    /// </summary>
    int TimeOut { get; }

    /// <summary>
    /// Get the connection asociate whith this thread. This method is not ThreadSafe.
    /// </summary>
    /// <returns></returns>
    IDbConnection GetDbConnection();
    /// <summary>
    /// Create a new connection for the operation.
    /// </summary>
    /// <returns></returns>
    IDbConnection CreateNewDbConnection();
}
