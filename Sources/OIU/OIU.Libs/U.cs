namespace OIU.Libs;

/// <summary>
/// 
/// </summary>
public sealed class U
{
    private static readonly UAssign _assign = new();

    /// <summary>
    /// 
    /// </summary>
    public static UAssign Assign => _assign;

    #region Nested Classes
    /// <summary>
    /// 
    /// </summary>
    public sealed class UAssign { internal UAssign() { } }
    #endregion
}
