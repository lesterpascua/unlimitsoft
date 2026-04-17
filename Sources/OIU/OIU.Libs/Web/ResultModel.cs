namespace OIU.Libs.Web;


/// <summary>
/// 
/// </summary>
/// <param name="Success"></param>
public sealed record ResultModel(bool Success)
{
    /// <summary>
    /// 
    /// </summary>
    public static readonly ResultModel Ok = new(true);
    /// <summary>
    /// 
    /// </summary>
    public static readonly ResultModel Err = new(false);
}
