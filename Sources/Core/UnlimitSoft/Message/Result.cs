using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;

namespace UnlimitSoft.Message;


/// <summary>
/// 
/// </summary>
public interface IResult
{
    /// <summary>
    /// Indicate if the operation finish successfully
    /// </summary>
    bool IsSuccess { get; }
    /// <summary>
    /// Error asociate to the result
    /// </summary>
    IResponse? Error { get; }

    /// <summary>
    /// Result if no error detected
    /// </summary>
    object? GetValue();
}
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



    /// <summary>
    /// Return a result from success
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [Obsolete("Use Ok")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResponse> FromOk<TResponse>(TResponse? value) => new(value, null);
    /// <summary>
    /// Return a result from error
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    [Obsolete("Use Err")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResponse> FromError<TResponse>(IResponse error) => new(default, error);
    /// <summary>
    /// Return a result from error
    /// </summary>
    /// <param name="err"></param>
    /// <param name="field"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    [Obsolete("Use Err")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResponse> FromError<TResponse, T>(T err, string? field = null, HttpStatusCode code = HttpStatusCode.BadRequest)
        where T : struct, Enum
    {
        return FromError<TResponse>([$"{err:D}"], field, code);
    }
    /// <summary>
    /// Return a result from error
    /// </summary>
    /// <param name="err"></param>
    /// <param name="field"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    [Obsolete("Use Err")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResponse> FromError<TResponse>(string err, string? field = null, HttpStatusCode code = HttpStatusCode.BadRequest)
    {
        return FromError<TResponse>([err], field, code);
    }
    /// <summary>
    /// Return a result from error
    /// </summary>
    /// <param name="errs"></param>
    /// <param name="field"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    [Obsolete("Use Err")]
    public static Result<TResponse> FromError<TResponse>(string[] errs, string? field = null, HttpStatusCode code = HttpStatusCode.BadRequest)
    {
        field ??= string.Empty;

        var body = new Dictionary<string, string[]> { [field] = errs };
        var result = new ErrorResponse(code, body);

        return FromError<TResponse>(result);
    }
}
/// <summary>
/// 
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public readonly struct Result<TResponse> : IResult
{
    /// <summary>
    /// Initialize Result
    /// </summary>
    /// <param name="value"></param>
    /// <param name="error"></param>
    internal Result(TResponse? value, IResponse? error)
    {
        Value = value;
        Error = error;
    }

    /// <summary>
    /// Result if no error detected
    /// </summary>
    public TResponse? Value { get; }
    /// <summary>
    /// Error asociate to the result
    /// </summary>
    public IResponse? Error { get; }
    /// <inheritdoc />
    public bool IsSuccess => Error is null;

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
