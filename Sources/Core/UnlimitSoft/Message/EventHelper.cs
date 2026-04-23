using System;
using System.Reflection;

namespace UnlimitSoft.Message;


/// <summary>
/// 
/// </summary>
public static class EventHelper
{
    /// <summary>
    /// Get the event name, if the class has a public static const field named "EventName" of type string, it will return its value, otherwise it will return the full name of the class.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetName(Type type)
    {
        var field = type.GetField("EventName", BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField);
        if (field is not null && field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
            return (string?)field.GetRawConstantValue() ?? type.FullName ?? type.Name;

        return type.FullName ?? type.Name;
    }
}
