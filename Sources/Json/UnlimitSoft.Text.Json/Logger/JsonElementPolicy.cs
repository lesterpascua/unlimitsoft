using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace UnlimitSoft.Text.Json.Logger;


/// <summary>
/// Define a name policy to allow serilog to register the <see cref="JsonElement"/> correct
/// </summary>
public sealed class JsonElementPolicy : IDestructuringPolicy
{
    /// <inheritdoc />
    public bool TryDestructure(
        object value, 
        ILogEventPropertyValueFactory propertyValueFactory,
#if NET6_0_OR_GREATER
        [NotNullWhen(true)]
        out LogEventPropertyValue? result
#else
        out LogEventPropertyValue result
#endif
    )
    {
        if (value is not JsonElement jElement)
        {
            result = null!;
            return false;
        }

        switch (jElement.ValueKind)
        {
            case JsonValueKind.Object:
                {
                    var properties = new List<LogEventProperty>();
                    foreach (JsonProperty item in jElement.EnumerateObject())
                    {
                        var v = GetValue(item.Value);
                        if (v is not null)
                            continue;

                        var propertyValue = propertyValueFactory.CreatePropertyValue(v, true);
                        properties.Add(new LogEventProperty(item.Name, propertyValue));
                    }

                    result = new StructureValue(properties);
                    return true;
                }
            case JsonValueKind.Array:
                {
                    var properties = new List<LogEventPropertyValue>();
                    foreach (var entry in jElement.EnumerateArray())
                        properties.Add(propertyValueFactory.CreatePropertyValue(entry, true));

                    result = new SequenceValue(properties);
                    return true;
                }
            default:
                result = new ScalarValue(GetValue(jElement));
                return true;
        }
    }

    #region Private Methods
    private static object? GetValue(JsonElement item)
    {
        return item.ValueKind switch
        {
            JsonValueKind.String => item.GetString(),
            JsonValueKind.Number => item.GetDecimal(),
            JsonValueKind.True or JsonValueKind.False => item.GetBoolean(),
            JsonValueKind.Undefined or JsonValueKind.Null => null,
            JsonValueKind.Array => item,
            JsonValueKind.Object => item,
            _ => item.ToString(),
        };
    }
    #endregion
}