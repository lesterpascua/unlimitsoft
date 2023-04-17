using Newtonsoft.Json.Linq;
using System;

namespace UnlimitSoft.Newtonsoft;


/// <summary>
/// 
/// </summary>
public sealed class JTokenConverter : System.Text.Json.Serialization.JsonConverter<JToken>
{
    /// <inheritdoc />
    public override void Write(System.Text.Json.Utf8JsonWriter writer, JToken value, System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteRawValue(value.ToString(global::Newtonsoft.Json.Formatting.None));
    }
    /// <inheritdoc />
    public override JToken? Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (str is not null)
            return JToken.Parse(str);
        return JToken.Parse("null");
    }
}