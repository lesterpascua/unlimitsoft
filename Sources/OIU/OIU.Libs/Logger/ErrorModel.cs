using System.ComponentModel;
using System.Reflection;

namespace OIU.Libs.Logger;


/// <summary>
/// 
/// </summary>
/// <param name="Code"></param>
/// <param name="Name"></param>
/// <param name="Description"></param>
public sealed record ErrorModel(int Code, string Name, string Description)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    public static ErrorModel[] GetErrores(Type[] types)
    {
        var errors = new List<ErrorModel>();
        foreach (var type in types)
        {
            foreach (var field in type.GetFields())
            {
                var value = Convert.ToString(field.GetValue(field))!;

                var code = int.Parse(value);
                var name = field.Name;
                var description = field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? field.Name;

                errors.Add(new ErrorModel(code, name, description));
            }
        }

        return [.. errors.OrderBy(k => k.Code)];
    }
}