using Microsoft.OpenApi.Models;
using SoftUnlimit.Web.Client;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SoftUnlimit.Cloud.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public class SwaggerExcludeFilter : ISchemaFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
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
}
