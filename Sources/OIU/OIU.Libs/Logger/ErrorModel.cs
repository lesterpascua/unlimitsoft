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
            if (type.IsEnum)
            {
                PopulateFromEnum(errors, type);
                continue;
            }
            PopulateFromClass(errors, type);
        }

        return [.. errors.OrderBy(k => k.Code)];
    }

    private static void PopulateFromEnum(List<ErrorModel> errors, Type type)
    {
        foreach (var value in Enum.GetValues(type))
        {
            var code = value.GetHashCode();
            var name = Enum.GetName(type, value) ?? throw new InvalidProgramException("No name asociate with the enumerator");

            var description = name;
            var memberInfo = type.GetMember(name).FirstOrDefault();
            if (memberInfo is not null)
            {
                var attr = memberInfo
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .FirstOrDefault();
                if (attr is not null)
                    description = ((DescriptionAttribute)attr).Description;
            }
            errors.Add(new OIU.Libs.Logger.ErrorModel(code, name, description));
        }
    }
    private static void PopulateFromClass(List<ErrorModel> errors, Type type)
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
}