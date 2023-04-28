using Newtonsoft.Json.Linq;
using Serilog.Core;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace UnlimitSoft.NewtonSoft.Logger;


/// <summary>
/// Define a name policy to allow serilog to register the <see cref="JsonElement"/> correct
/// </summary>
public sealed class JObjectPolicy : IDestructuringPolicy
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
        if (value is not JObject jObject)
        {
            result = null!;
            return false;
        }

        var properties = new List<Serilog.Events.LogEventProperty>();
        foreach (var entry in jObject)
        {
            var v = entry.Value switch
            {
                JValue jValue => jValue.Value,
                JArray jArray => jArray,
                _ => entry.Value?.ToString(),
            };

            if (v is null)
                continue;

            var propertyValue = propertyValueFactory.CreatePropertyValue(v, true);
            properties.Add(new Serilog.Events.LogEventProperty(entry.Key, propertyValue));
        }
        result = new Serilog.Events.StructureValue(properties);
        return true;
    }
}