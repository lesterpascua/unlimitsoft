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