using Microsoft.EntityFrameworkCore;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Web.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Data.EntityFramework
{
    /// <summary>
    /// 
    /// </summary>
    public static class IQueryableExtensions
    {
        /// <summary>
        /// Execute query for search.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="factory">Allow create a parallel connection to execute count query to get the total amount of result.</param>
        /// <param name="pagging"></param>
        /// <param name="ordered"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static ValueTask<(int, TEntity[])> ToSearchAsync<TEntity>(this IQueryable<TEntity> @this, IDbConnectionFactory factory, Paging pagging, IEnumerable<ColumnName> ordered, CancellationToken ct = default)
            where TEntity : class
        {
            var countCommandText = @this.GroupBy(p => 1).Select(s => s.Count()).ToQueryString();
            var countTask = Task.Run(() =>
            {
                using var connection = factory.CreateNewDbConnection();
                var sqlCommand = connection.CreateCommand();
                sqlCommand.CommandTimeout = factory.TimeOut;
                sqlCommand.CommandText = countCommandText;

                if (connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();

                var result = (int)sqlCommand.ExecuteScalar();
                return result;
            });

            var searchTask = @this.ApplySearch(pagging, ordered).ToArrayAsync(ct);

            Task.WaitAll(new Task[] { searchTask, countTask }, ct);
            return ValueTask.FromResult((countTask.Result, searchTask.Result));
        }
    }
}
