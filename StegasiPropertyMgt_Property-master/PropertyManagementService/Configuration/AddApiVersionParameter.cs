using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PropertyManagementService.Configuration;

public class AddApiVersionParameter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiVersion = context.ApiDescription.GetApiVersion();
        if (apiVersion == null)
            return;

        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "api-version",
            In = ParameterLocation.Header,
            Description = "API version",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new OpenApiString(apiVersion.ToString())
            }
        });
    }
} 