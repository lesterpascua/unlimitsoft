namespace UnlimitSoft.Message;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public readonly struct Result<TResponse>
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
}
