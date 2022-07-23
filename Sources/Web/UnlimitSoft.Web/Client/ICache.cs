using System;
using System.Threading.Tasks;

namespace UnlimitSoft.Web.Client;


/// <summary>
/// 
/// </summary>
public interface ICache
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TEntry"></typeparam>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    ValueTask<TResult> GetOrCreateAsync<TResult, TEntry>(object key, Func<TEntry, Task<TResult>> factory);
}
