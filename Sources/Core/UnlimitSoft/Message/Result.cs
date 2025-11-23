using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;

namespace UnlimitSoft.Message;


/// <summary>
/// 
/// </summary>
public readonly struct Result
{
    /// <summary>
    /// Return a result from success
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResponse> Ok<TResponse>(TResponse? value) => new(value, null);

    /// <summary>
    /// Return a result from error
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResponse> Err<TResponse>(IResponse error) => new(default, error);
    /// <summary>
    /// Return a result from error
    /// </summary>
    /// <param name="err"></param>
    /// <param name="field"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResponse> Err<TResponse, T>(T err, string? field = null, HttpStatusCode code = HttpStatusCode.BadRequest) where T : struct, Enum => Err<TResponse>([$"{err:D}"], field, code);
    /// <summary>
    /// Return a result from error
    /// </summary>
    /// <param name="err"></param>
    /// <param name="field"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResponse> Err<TResponse>(string err, string? field = null, HttpStatusCode code = HttpStatusCode.BadRequest) => Err<TResponse>([err], field, code);
    /// <summary>
    /// Return a result from error
    /// </summary>
    /// <param name="errs"></param>
    /// <param name="field"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static Result<TResponse> Err<TResponse>(string[] errs, string? field = null, HttpStatusCode code = HttpStatusCode.BadRequest)
    {
        field ??= string.Empty;

        var body = new Dictionary<string, string[]> { [field] = errs };
        var result = new ErrorResponse(code, body);

        return Err<TResponse>(result);
    }
}
/// <summary>
/// 
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public readonly struct Result<TResponse> : IResult
{
    private readonly TResponse? _value;
    private readonly IResponse? _error;

    /// <summary>
    /// Initialize Result
    /// </summary>
    /// <param name="value"></param>
    /// <param name="error"></param>
    internal Result(TResponse? value, IResponse? error)
    {
        _value = value;
        _error = error;
    }

    /// <summary>
    /// Result if no error detected
    /// </summary>
    public TResponse? Value => _value;
    /// <summary>
    /// Error asociate to the result
    /// </summary>
    public IResponse? Error => _error;
    /// <inheritdoc />
    public bool IsSuccess => _error is null;

    /// <summary>
    /// Destructuring result
    /// </summary>
    /// <param name="value"></param>
    /// <param name="error"></param>
    public void Deconstruct(out TResponse? value, out IResponse? error)
    {
        value = Value;
        error = Error;
    }

    /// <inheritdoc />
    public override string ToString() => $"Value: {Value}, Error: {Error}";

    #region Private Methods
    object? IResult.GetValue() => Value;
    #endregion
}
/// <summary>
/// Extension method for <see cref="Result{TResponse}"/>
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Return a result from success
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TOut> ErrTo<TIn, TOut>(this in Result<TIn> value) => Result.Err<TOut>(value.Error!);
}