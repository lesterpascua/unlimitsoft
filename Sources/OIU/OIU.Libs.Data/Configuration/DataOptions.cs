using System.Reflection;

namespace OIU.Libs.Data.Configuration;


/// <summary>
/// 
/// </summary>
public sealed class DataOptions
{
    /// <summary>
    /// 
    /// </summary>
    public Assembly[] EntityTypeBuilders { get; set; } = [];
}
