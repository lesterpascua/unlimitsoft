using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query.Compliance
{
    /// <summary>
    /// This interface is only for reference please inplement ICommandCompliance in TCommand to correct resolution using DPI
    /// </summary>
    public interface IQueryCompliance
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<QueryResponse> ExecuteAsync(IQuery args);
    }
    /// <summary>
    /// 
    /// </summary>
    public interface IQueryCompliance<TQuery> : IQueryCompliance
        where TQuery : IQuery
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<QueryResponse> ExecuteAsync(TQuery args);
    }
}
