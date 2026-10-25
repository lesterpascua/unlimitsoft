using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.RegularExpressions;

namespace OIU.Libs.AspNet.Swagger;


/// <summary>
/// 
/// </summary>
internal sealed class TagDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var regex = new Regex("/api/v[0-9]+/", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10));

        var aux = new List<KeyValuePair<string, IOpenApiPathItem>>();
        foreach (var path in swaggerDoc.Paths)
        {
            aux.Add(new KeyValuePair<string, IOpenApiPathItem>(path.Key.ToLowerInvariant(), path.Value));

            if (path.Value.Operations is null)
                continue;

            foreach (var operation in path.Value.Operations)
            {
                if (operation.Value.Tags is null)
                    continue;

                foreach (var tag in operation.Value.Tags)
                {
                    var match = regex.Match(path.Key);
                    if (!match.Success)
                        continue;

                    var group = match.Groups[0];
                    var start = group.Index + group.Length;

                    var prefix = path.Key[start..];
                    var end = prefix.IndexOf('/');

                    prefix = end != -1 ? prefix[..end] : string.Empty;
                    var isMatch =
                        prefix.Equals("sys", StringComparison.InvariantCultureIgnoreCase) ||
                        prefix.Equals("admin", StringComparison.InvariantCultureIgnoreCase) ||
                        prefix.Equals("m2m", StringComparison.InvariantCultureIgnoreCase);

                    //tag.Name = isMatch ? $"{prefix.ToLower()}/{tag.Name}" : $"_self/{tag.Name}";
                }
            }
        }
        var paths = new OpenApiPaths();
        foreach (var path in aux.OrderBy(OrderFunc))
            paths.Add(path.Key, path.Value);

        swaggerDoc.Paths = paths;
    }
    /// <summary>
    /// Remove the document tags not referenced by any operation of the document (tags of endpoints living in other versions).
    /// </summary>
    /// <param name="swaggerDoc"></param>
    public static void RemoveUnusedTags(OpenApiDocument swaggerDoc)
    {
        if (swaggerDoc.Tags is null)
            return;

        var used = swaggerDoc.Paths
            .SelectMany(path => path.Value.Operations?.Values ?? Enumerable.Empty<OpenApiOperation>())
            .SelectMany(operation => operation.Tags ?? Enumerable.Empty<OpenApiTagReference>())
            .Select(tag => tag.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var tag in swaggerDoc.Tags.Where(tag => tag.Name is null || !used.Contains(tag.Name)).ToList())
            swaggerDoc.Tags.Remove(tag);
    }

    #region Private Methods
    private static string? OrderFunc(KeyValuePair<string, IOpenApiPathItem> item)
    {
        var ope = item.Value.Operations?.FirstOrDefault();
        if (ope?.Value is null)
            return null;
        return ope.Value.Value.Tags?.FirstOrDefault()?.Name;
    }
    #endregion
}