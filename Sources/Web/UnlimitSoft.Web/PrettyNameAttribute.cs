using System;

namespace UnlimitSoft.Web;


/// <summary>
/// Allow stablish a prety name for enumerator
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
[Obsolete("use specifx attibute")]
public class PrettyNameAttribute : Attribute
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    public PrettyNameAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// 
    /// </summary>
    public string Name { get; }
}
