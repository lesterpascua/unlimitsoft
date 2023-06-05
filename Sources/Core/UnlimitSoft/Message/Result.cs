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
/// <typeparam name="TResponse"></typeparam>
public readonly struct Result<TResponse> : IResult
{
    /// <summary>
    /// Initialize Result
    /// </summary>
    /// <param name="value"></param>
    /// <param name="error"></param>
    public Result(TResponse? value, IResponse? error)
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
