using System;
using System.Linq;
using System.Threading;

namespace UnlimitSoft.Message;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public struct AgregateResult<TResponse> : IResult
{
    private Result<TResponse>[]? _value;
    private readonly Result<TResponse>[] _responses;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="responses"></param>
    public AgregateResult(Result<TResponse>[] responses)
    {
        _responses = responses;
    }

    /// <summary>
    /// Return all the responses from the multiple handlers.
    /// </summary>
    public Result<TResponse>[]? Value
    {
        get
        {
            if (_value is not null)
                return _value;
            Interlocked.CompareExchange(ref _value, [.. _responses.Where(x => x.IsSuccess)], null);
            return _value;
        }
    }

    /// <inheritdoc />
    public readonly bool IsSuccess => !Array.Exists(_responses, x => x.IsSuccess != true);
    /// <inheritdoc />
    public readonly IResponse? Error
    {
        get
        {
            var index = Array.FindIndex(_responses, x => x.IsSuccess != true);
            return (index != -1) ? _responses[index].Error : null;
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Value: {Value}, Error: {Error}";
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="error"></param>
    public void Deconstruct(out Result<TResponse>[]? value, out IResponse? error)
    {
        value = Value;
        error = Error;
    }

    /// <inheritdoc />
    readonly object? IResult.GetValue() => Value;
};