﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Web.Model;

namespace UnlimitSoft.Data.EntityFramework;


/// <summary>
/// 
/// </summary>
public static class IQueryableExtensions
{
    /// <summary>
    /// Execute query for search.
    /// </summary>
    /// <remarks>
    /// For search need execute 2 query 1 for get amount other for get the element, this will execute secuencial.
    /// </remarks>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="this"></param>
    /// <param name="pagging"></param>
    /// <param name="ordered"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<(int, TEntity[])> ToSearchAsync<TEntity>(this IQueryable<TEntity> @this, Paging pagging, IEnumerable<ColumnName> ordered, CancellationToken ct = default)
        where TEntity : class
    {
        var count = await @this.CountAsync(ct);
        var search = await @this.ApplySearch(pagging, ordered).ToArrayAsync(ct);

        return (count, search);
    }
    /// <summary>
    /// Execute query for search.
    /// </summary>
    /// <remarks>
    /// For search need execute 2 query 1 for get amount other for get the element, this will execute secuencial.
    /// </remarks>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="this"></param>
    /// <param name="pagging"></param>
    /// <param name="ordered"></param>
    /// <param name="selector"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<(int, TOut[])> ToSearchAsync<TIn, TOut>(this IQueryable<TIn> @this, Paging pagging, IEnumerable<ColumnName> ordered, Expression<Func<TIn, TOut>> selector, CancellationToken ct = default)
        where TIn : class
        where TOut : class
    {
        var count = await @this.CountAsync(ct);
        var search = await @this.ApplySearch(pagging, ordered).Select(selector).ToArrayAsync(ct);

        return (count, search);
    }
    /// <summary>
    /// Execute query for search.
    /// </summary>
    /// <remarks>
    /// For search need execute 2 query 1 for get amount other for get the element. For beeter optimization execute both in parallel.
    /// </remarks>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="this"></param>
    /// <param name="factory">Allow create a parallel connection to execute count query to get the total amount of result.</param>
    /// <param name="pagging"></param>
    /// <param name="ordered"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static ValueTask<(long, TEntity[])> ToSearchAsync<TEntity>(this IQueryable<TEntity> @this, IDbConnectionFactory factory, Paging pagging, IEnumerable<ColumnName> ordered, CancellationToken ct = default)
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

            var result = sqlCommand.ExecuteScalar();
            return result is not null ? Convert.ToInt64(result) : 0;
        });

        var searchTask = @this.ApplySearch(pagging, ordered).ToArrayAsync(ct);

        Task.WaitAll([searchTask, countTask], ct);
        return ValueTask.FromResult((countTask.Result, searchTask.Result));
    }
    /// <summary>
    /// Execute query for search.
    /// </summary>
    /// <remarks>
    /// For search need execute 2 query 1 for get amount other for get the element. For beeter optimization execute both in parallel.
    /// </remarks>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="this"></param>
    /// <param name="factory">Allow create a parallel connection to execute count query to get the total amount of result.</param>
    /// <param name="pagging"></param>
    /// <param name="ordered"></param>
    /// <param name="selector"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static ValueTask<(long, TOut[])> ToSearchAsync<TIn, TOut>(this IQueryable<TIn> @this, IDbConnectionFactory factory, Paging pagging, IEnumerable<ColumnName> ordered, Expression<Func<TIn, TOut>> selector, CancellationToken ct = default)
        where TIn : class
        where TOut : class
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

            var result = sqlCommand.ExecuteScalar();
            return result is not null ? Convert.ToInt64(result) : 0;
        });

        var searchTask = @this.ApplySearch(pagging, ordered).Select(selector).ToArrayAsync(ct);

        Task.WaitAll([searchTask, countTask], ct);
        return ValueTask.FromResult((countTask.Result, searchTask.Result));
    }
}
