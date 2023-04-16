namespace UnlimitSoft.Json;


/// <summary>
/// Diferent types of json
/// </summary>
public enum TokenType
{
    /// <summary>
    /// Token is array
    /// </summary>
    Array = 1,
    /// <summary>
    /// Token is an object
    /// </summary>
    Object = 2,
    /// <summary>
    /// Token is null element
    /// </summary>
    Null = 3,
    /// <summary>
    /// Token is string element
    /// </summary>
    String = 4,
    /// <summary>
    /// Token is not defined
    /// </summary>
    Undefined = 50
}