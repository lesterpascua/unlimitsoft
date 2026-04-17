using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace OIU.Libs.Data;


/// <summary>
/// Context used to read operations only type inherith from <see cref="DbContext"/> can implement this interface
/// </summary>
public interface IDbContextRead : IInfrastructure<IServiceProvider>;
/// <summary>
/// Context used to read operations
/// </summary>
public abstract class DbContextRead : OIUDbContext, IDbContextRead
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="configurationType"></param>
    /// <param name="options"></param>
    protected DbContextRead(Type configurationType, [NotNull] DbContextOptions options) : base(configurationType, options) { }
}