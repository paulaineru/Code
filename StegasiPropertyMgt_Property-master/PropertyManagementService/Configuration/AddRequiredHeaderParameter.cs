using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PropertyManagementService.Configuration;

public class AddRequiredHeaderParameter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Correlation-ID",
            In = ParameterLocation.Header,
            Description = "Correlation ID for request tracking",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "uuid"
            }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-API-Key",
            In = ParameterLocation.Header,
            Description = "API Key for service authentication",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }
} 