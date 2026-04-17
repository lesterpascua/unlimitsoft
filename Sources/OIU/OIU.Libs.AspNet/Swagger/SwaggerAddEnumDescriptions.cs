using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OIU.Libs.AspNet.Swagger;


/// <summary>
/// Include enums description in generate documentation
/// </summary>
internal sealed class SwaggerAddEnumDescriptions : IDocumentFilter
{
    private readonly Dictionary<string, Type> _enums;

    /// <summary>
    /// 
    /// </summary>
    public SwaggerAddEnumDescriptions()
    {
        var prefix = new string[] { "Zafe.", "OIU.Libs." };
        var loaded = AppDomain.CurrentDomain.GetAssemblies();
        _enums = loaded
            .Where(a => prefix.Any(x => a.FullName!.StartsWith(x)))
            .SelectMany(t => t.GetTypes())
            .Where(p => p.IsEnum)
            .ToDictionary(k => k.FullName!);
    }

    /// <inheritdoc />
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (swaggerDoc.Components?.Schemas is null)
            return;

        foreach (var schemaDictionaryItem in swaggerDoc.Components.Schemas)
        {
            var schema = schemaDictionaryItem.Value;
            if (schema.Enum is null || !schema.Enum.Any() || !_enums.TryGetValue(schemaDictionaryItem.Key, out var enumType))
                continue;

            ;
            //schema.Example = null; // new JsonNode(DescribeEnum(enumType, schema.Enum.OfType<OpenApiInteger>()));
        }
    }

    #region Private Methods
    //private static string DescribeEnum(Type enumType, IEnumerable<OpenApiInteger> enums)
    //{
    //    var sb = new StringBuilder();
    //    foreach (var enumOption in enums)
    //    {
    //        var name = Enum.GetName(enumType, enumOption.Value);
    //        sb.Append($"{enumOption.Value} = {name}, ");
    //    }
    //    return sb.ToString(0, sb.Length - 2);
    //}
    #endregion
}