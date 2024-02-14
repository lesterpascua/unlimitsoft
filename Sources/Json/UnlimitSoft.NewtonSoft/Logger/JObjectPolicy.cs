using Newtonsoft.Json.Linq;
using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UnlimitSoft.NewtonSoft.Logger;


/// <summary>
/// Define a name policy to allow serilog to register the System.Text.Json.JsonElement correct
/// </summary>
public sealed class JObjectPolicy : IDestructuringPolicy
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
        if (value is not JToken jToken)
        {
            result = null!;
            return false;
        }

        switch (jToken.Type)
        {
            case JTokenType.Object:
                {
                    var properties = new List<LogEventProperty>();
                    foreach (var entry in (JObject)jToken)
                    {
                        var v = GetValue(entry.Value);
                        if (v is null)
                            continue;

                        var propertyValue = propertyValueFactory.CreatePropertyValue(v, true);
                        properties.Add(new LogEventProperty(entry.Key, propertyValue));
                    }

                    result = new StructureValue(properties);
                    return true;
                }
            case JTokenType.Array:
                {
                    var properties = new List<LogEventPropertyValue>();
                    foreach (var entry in (JArray)jToken)
                        properties.Add(propertyValueFactory.CreatePropertyValue(entry, true));
                    result = new SequenceValue(properties);
                    return true;
                }
            default:
                result = new ScalarValue(GetValue(jToken.First));
                return true;
        }
    }

    #region Private Methods
    private static object? GetValue(JToken? token)
    {
        if (token is JValue jValue)
            return jValue.Value;

        return token?.Type switch
        {
            JTokenType.Array => token,
            JTokenType.Object => token,
            JTokenType.Property => token,
            JTokenType.Constructor => token,
            //JTokenType.Uri => ((JValue)token).Value,
            //JTokenType.Boolean => ((JValue)token).Value,
            //JTokenType.Integer or JTokenType.Float => ((JValue)token).Value,
            //JTokenType.String or JTokenType.Comment => ((JValue)token).Value,
            //JTokenType.Date or JTokenType.TimeSpan => ((JValue)token).Value,
            JTokenType.Undefined or JTokenType.Null => null,
            JTokenType.Raw or _ => token?.ToString(),
        };
    }
    #endregion
}