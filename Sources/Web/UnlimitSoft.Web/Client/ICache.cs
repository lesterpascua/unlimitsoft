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
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    ValueTask<TResult> GetOrCreateAsync<TResult>(object key, Func<object, Task<TResult>> factory);
}
