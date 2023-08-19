using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UnlimitSoft.Text.Json;


/// <summary>
/// 
/// </summary>
public sealed class MethodBaseJsonConverter : JsonConverter<MethodBase>
{
    /// <inheritdoc />
    public override MethodBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => null;
    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, MethodBase value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}
