using Serilog.Core;
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
        out Serilog.Events.LogEventPropertyValue? result
#else
        out Serilog.Events.LogEventPropertyValue result
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
                    var properties = new List<Serilog.Events.LogEventProperty>();
                    foreach (var entry in jElement.EnumerateObject())
                    {
                        object? v = entry.Value.ValueKind switch
                        {
                            JsonValueKind.Array => entry.Value,
                            JsonValueKind.Object => entry.Value,
                            JsonValueKind.Number => entry.Value.GetInt64(),
                            JsonValueKind.String => entry.Value.GetString(),
                            JsonValueKind.False or JsonValueKind.True => entry.Value.GetBoolean(),
                            JsonValueKind.Null or JsonValueKind.Undefined => null,
                            _ => entry.Value.ToString(),
                        };
                        if (v is null)
                            continue;

                        var propertyValue = propertyValueFactory.CreatePropertyValue(v, true);
                        properties.Add(new Serilog.Events.LogEventProperty(entry.Name, propertyValue));
                    }

                    result = new Serilog.Events.StructureValue(properties);
                    return true;
                }
            case JsonValueKind.Array:
                {
                    var properties = new List<Serilog.Events.LogEventPropertyValue>();
                    foreach (var entry in jElement.EnumerateArray())
                        properties.Add(propertyValueFactory.CreatePropertyValue(entry, true));

                    result = new Serilog.Events.SequenceValue(properties);
                    return true;
                }
            case JsonValueKind.Number:
                result = new Serilog.Events.ScalarValue(jElement.GetInt64());
                return true;
            case JsonValueKind.String:
                result = new Serilog.Events.ScalarValue(jElement.GetString());
                return true;
            case JsonValueKind.False or JsonValueKind.True:
                result = new Serilog.Events.ScalarValue(jElement.GetBoolean());
                return true;
            case JsonValueKind.Null or JsonValueKind.Undefined:
                result = new Serilog.Events.ScalarValue(null);
                return true;
            default:
                result = new Serilog.Events.ScalarValue(jElement.GetString());
                return true;
        }
    }
}