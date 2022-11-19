using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnlimitSoft.CQRS.Cache;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.Logging;
using UnlimitSoft.CQRS.Message;
using UnlimitSoft.CQRS.Query.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.CQRS.Query;


/// <summary>
/// Query provider dispatcher using and standard IServiceProvider to locate the QueryHandler associate with a query.
/// </summary>
public class ServiceProviderQueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _provider;

    private readonly bool _validate;
    private readonly Func<IQuery, Func<IQuery, CancellationToken, Task<IQueryResponse>>, CancellationToken, Task<IQueryResponse>>? _preeDispatch;

    private readonly string? _invalidArgumendText;
    private readonly Func<IEnumerable<ValidationFailure>, IDictionary<string, string[]>>? _errorTransforms;

    private readonly Dictionary<Type, QueryMetadata> _cache;
    private readonly ILogger<ServiceProviderQueryDispatcher>? _logger;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="validate"></param>
    /// <param name="errorTransforms"></param>
    /// <param name="invalidArgumendText"></param>
    /// <param name="preeDispatch"></param>
    /// <param name="logger"></param>
    public ServiceProviderQueryDispatcher(
        IServiceProvider provider, 
        bool validate = true,
        Func<IEnumerable<ValidationFailure>, IDictionary<string, string[]>>? errorTransforms = null, 
        string? invalidArgumendText = null,
        Func<IQuery, Func<IQuery, CancellationToken, Task<IQueryResponse>>, CancellationToken, Task<IQueryResponse>>? preeDispatch = null, 
        ILogger<ServiceProviderQueryDispatcher>? logger = null
    )
    {
        _provider = provider;
        _validate = validate;
        _errorTransforms = errorTransforms ?? ServiceProviderCommandDispatcher.DefaultErrorTransforms;
        _invalidArgumendText = invalidArgumendText;
        _preeDispatch = preeDispatch;
        _logger = logger;
        _cache = new Dictionary<Type, QueryMetadata>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IQueryResponse> DispatchAsync<TResult>(IQuery query, CancellationToken ct = default)
    {
        if (_preeDispatch is null)
            return await RunAsync<TResult>(query, ct);

        return await _preeDispatch(query, RunAsync<TResult>, ct);
    }

    #region Private Methods
    private async Task<IQueryResponse> RunAsync<TResult>(IQuery query, CancellationToken ct)
    {
        _logger?.ServiceProviderQueryDispatcher_ProcessQuery(query);

        //
        // Get handler and execute command.
        var queryType = query.GetType();
        var entityType = typeof(TResult);
        _logger?.ServiceProviderQueryDispatcher_ExecuteCommandType(queryType);

        var handler = GetQueryHandler(_provider, entityType, queryType, out var metadata);
        if (_validate)
        {
            IQueryResponse? response;

            response = await RunValidationAsync(handler, query, queryType, metadata, ct);
            if (response is not null)
                return response;

            response = await ComplianceAsync(handler, query, queryType, metadata, ct);
            if (response is not null)
                return response;
        }

        var result = await HandlerAsync<TResult>(handler, query, queryType, ct);
        return query.OkResponse(result);
    }
    /// <summary>
    /// Get command handler and metadata asociate to a command.
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="entity"></param>
    /// <param name="queryType"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    private IQueryHandler GetQueryHandler(IServiceProvider provider, Type entity, Type queryType, out QueryMetadata metadata)
    {
        if (_cache.TryGetValue(queryType, out metadata))
            return (IQueryHandler)provider.GetRequiredService(metadata.QueryHandler);

        lock (_cache)
            if (!_cache.TryGetValue(queryType, out metadata))
            {
                _cache.Add(queryType, metadata = new QueryMetadata());

                // Handler
                metadata.QueryHandler = typeof(IQueryHandler<,>).MakeGenericType(entity, queryType);
                var handler = (IQueryHandler)provider.GetRequiredService(metadata.QueryHandler);

                // Features implemented by this command
                var interfaces = handler.GetType().GetInterfaces();

                // Validator
                var validationHandlerType = typeof(IQueryHandlerValidator<>).MakeGenericType(queryType);
                if (interfaces.Any(type => type == validationHandlerType))
                    metadata.ValidatorType = typeof(QueryValidator<>).MakeGenericType(queryType);

                // Compliance
                var complianceHandlerType = typeof(IQueryHandlerCompliance<>).MakeGenericType(queryType);
                if (interfaces.Any(type => type == complianceHandlerType))
                    metadata.HasCompliance = true;

                return handler;
            }

        // Resolve service
        return (IQueryHandler)provider.GetRequiredService(metadata.QueryHandler);
    }
    /// <summary>
    /// Execute query handler
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="handler"></param>
    /// <param name="query"></param>
    /// <param name="queryType"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async ValueTask<TEntity> HandlerAsync<TEntity>(IQueryHandler handler, IQuery query, Type queryType, CancellationToken ct)
    {
        var method = CacheDispatcher.GetQueryHandler<TEntity>(queryType, handler);
        var result = (Task<TEntity>)method(handler, query, ct);

        return await result;
    }
    /// <summary>
    /// Execute Query compliance
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="query"></param>
    /// <param name="queryType"></param>
    /// <param name="metadata"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async ValueTask<IQueryResponse?> ComplianceAsync(IQueryHandler handler, IQuery query, Type queryType, QueryMetadata metadata, CancellationToken ct)
    {
        if (!metadata.HasCompliance)
        {
            _logger?.ServiceProviderQueryDispatcher_QueryNotHandlerImplementCompliance(query);
            return null;
        }

        _logger?.ServiceProviderQueryDispatcher_QueryHandlerImplementCompliance(query);

        var method = CacheDispatcher.GetQueryCompliance(queryType, handler);
        var response = await method(handler, query, ct);
        if (!response.IsSuccess)
            return response;

        return null;
    }
    /// <summary>
    /// Execute validator
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="query"></param>
    /// <param name="queryType"></param>
    /// <param name="metadata"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async ValueTask<IQueryResponse?> RunValidationAsync(IQueryHandler handler, IQuery query, Type queryType, QueryMetadata metadata, CancellationToken ct)
    {
        if (metadata.ValidatorType is null)
        {
            _logger?.ServiceProviderQueryDispatcher_QueryNotHandlerImplementValidation(query);
            return null;
        }

        _logger?.ServiceProviderQueryDispatcher_QueryHandlerImplementValidation(query);

        var validator = (IValidator)Activator.CreateInstance(metadata.ValidatorType);
        var method = CacheDispatcher.GetQueryValidator(queryType, metadata.ValidatorType, handler);

        var response = await method(handler, query, validator, ct);
        if (!response.IsSuccess)
            return response;

        var valContext = new ValidationContext<IQuery>(query);
        var errors = await validator.ValidateAsync(valContext, ct);

        _logger?.ServiceProviderQueryDispatcher_EvaluateValidatorProcessResultErrors(errors);
        if (errors?.IsValid != false)
            return null;

        if (_errorTransforms is null)
            return query.BadResponse(errors.Errors, _invalidArgumendText);
        return query.BadResponse(_errorTransforms(errors.Errors), _invalidArgumendText);
    }
    #endregion

    #region Nested Classes
    private sealed class QueryMetadata
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Type ValidatorType;
        public Type QueryHandler;
        public bool HasCompliance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
    #endregion
}