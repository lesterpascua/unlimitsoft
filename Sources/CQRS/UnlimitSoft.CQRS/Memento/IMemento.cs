using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.CQRS.Memento;


/// <summary>
/// Allow build some entity using all event applied in the history. 
/// </summary>
/// <remarks>The event will be applied in the order of receive.</remarks>
/// <typeparam name="TEntity"></typeparam>
public interface IMemento<TEntity>
{
    /// <summary>
    /// Build entity in the moment of the version supplied
    /// </summary>
    /// <param name="id"></param>
    /// <param name="version">Version of the entity.</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<TEntity?> FindByVersionAsync(Guid id, long? version = null, CancellationToken ct = default);
    /// <summary>
    /// Build entity in the moment of the date supplied.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dateTime">Date where we need to check the entity.</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<TEntity?> FindByCreateAsync(Guid id, DateTime? dateTime = null, CancellationToken ct = default);
}