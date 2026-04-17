using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace OIU.Libs.Data;


/// <summary>
/// Context used to write operations only type inherith from <see cref="DbContext"/> can implement this interface
/// </summary>
public interface IDbContextWrite : IInfrastructure<IServiceProvider>;
/// <summary>
/// Context used to write operations
/// </summary>
public abstract class DbContextWrite : OIUDbContext, IDbContextWrite
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configurationType"></param>
    /// <param name="options"></param>
    protected DbContextWrite(Type configurationType, [NotNull] DbContextOptions options) : base(configurationType, options) { }
}