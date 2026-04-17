using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace OIU.Libs.Web;


/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Id">Entity identifier</param>
/// <param name="Name">Name of the entity entry</param>
/// <param name="Description">Field description</param>
public sealed record EnumModel<T>(T Id, string Name, string Description) where T : struct, Enum
{
    private static readonly Dictionary<Type, EnumModel<T>[]> _cache = [];

    /// <summary>
    /// sa
    /// </summary>
    /// <returns></returns>
    public static EnumModel<T>[] Get()
    {
        if (_cache.TryGetValue(typeof(T), out var data))
            return data;

        lock (Constants.Lock.Instance)
        {
            if (_cache.TryGetValue(typeof(T), out data))
                return data;

            data = [.. Enum
#if NETSTANDARD2_0
                .GetValues(typeof(T))
                .Cast<T>()
#else
                .GetValues<T>()
#endif
                .Select(s =>
                {
                    var rawName = s.ToString();
                    var name = typeof(T)
                        .GetMember(rawName)[0].GetCustomAttributes<DisplayAttribute>(false)
                        .FirstOrDefault()?
                        .Name;

                    var description = typeof(T)
                        .GetMember(s.ToString())[0]?.GetCustomAttributes<DescriptionAttribute>(false)
                        .FirstOrDefault()?
                        .Description;

                    return new EnumModel<T>(s, name ?? rawName, description!);
                })];

            _cache.Add(typeof(T), data);
        }
        return data;
    }
}
