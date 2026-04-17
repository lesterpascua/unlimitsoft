using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using UnlimitSoft.Message;

namespace OIU.Libs.AspNet.Swagger;


/// <summary>
/// 
/// </summary>
internal sealed class SwaggerExcludeFilter : ISchemaFilter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="context"></param>
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties == null || schema.Type == null)
            return;

        if (context.Type.IsGenericType && context.Type.GetGenericTypeDefinition() == typeof(Response<>))
        {
            schema.Properties.Remove("query");
            schema.Properties.Remove("command");
        }
    }
}
/*public class AddCustomQueryParams : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        Type? vmType;
#if NET9_0
        var aux = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<IParameterBindingMetadata>()
            .Where(x =>
            {
                return x.Name.EndsWith("VM", StringComparison.InvariantCultureIgnoreCase);
            })
            .FirstOrDefault();
        vmType = aux?.ParameterInfo.ParameterType;
#else
        vmType = null;
#endif

        if (vmType is null)
            return;

        // Flatten complex object properties into individual query parameters
        var schemaGenerator = context.SchemaGenerator;
        var schemaRepository = context.SchemaRepository;

        var schema = schemaGenerator.GenerateSchema(vmType, schemaRepository);
        schema = schemaRepository.Schemas[schema.Reference.Id];

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = $"vm", // e.g., vm.A or vm.O.P1
                In = ParameterLocation.Query,
                Required = false,
                Schema = schema,
                
            });
    }
}*/