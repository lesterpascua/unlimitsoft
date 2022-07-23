#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace UnlimitSoft.Web.Model;


/// <summary>
/// Model used to specified a id, name and some description.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class KeyName<TKey>
{
    /// <summary>
    /// 
    /// </summary>
    public TKey Id { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? Description { get; set; }
}
