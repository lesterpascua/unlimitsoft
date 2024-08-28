using System;
using System.Data;
using System.Threading;

namespace UnlimitSoft.Data;


/// <summary>
/// Represents a base repository for raw data access.
/// </summary>
public abstract class BaseRawRepository : IDbConnectionFactory, IDisposable
{
    private bool _disposed = false;
    private IDbConnection? _connection;

    private readonly string _connString;

    /// <summary>
    /// Finalizes an instance of the <see cref="BaseRawRepository"/> class.
    /// </summary>
    ~BaseRawRepository()
    {
        Dispose(false);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseRawRepository"/> class with the specified connection string.
    /// </summary>
    /// <param name="connString">The connection string.</param>
    public BaseRawRepository(string connString)
    {
        _connString = connString;
        TimeOut = GetConnectionTimeOut(connString);
    }

    /// <inheritdoc />
    public int TimeOut { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public IDbConnection GetDbConnection()
    {
        Interlocked.CompareExchange(ref _connection, CreateNewDbConnection(), null);
        return _connection;
    }

    #region Protected Methods
    /// <summary>
    /// Gets the connection string.
    /// </summary>
    protected string ConnectionString => _connString;

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="BaseRawRepository"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _connection?.Dispose();
        _disposed = true;
    }

    /// <inheritdoc />
    public abstract IDbConnection CreateNewDbConnection();
    /// <summary>
    /// Gets the connection timeout value based on the specified connection string.
    /// </summary>
    /// <param name="connString">The connection string.</param>
    /// <returns>The connection timeout value.</returns>
    protected abstract int GetConnectionTimeOut(string connString);
    #endregion
}
