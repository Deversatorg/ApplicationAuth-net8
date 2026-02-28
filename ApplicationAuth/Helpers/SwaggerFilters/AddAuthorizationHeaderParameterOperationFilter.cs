using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationAuth.Helpers
{
    /// <summary>
    /// Add 'Authorization' parameter when request must be authorized'
    /// </summary>
    public class AuthorizationHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            IList<Microsoft.AspNetCore.Mvc.Filters.FilterDescriptor> filterDescriptors = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            bool isAuthorized = filterDescriptors.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AuthorizeFilter);
            bool allowAnonymous = filterDescriptors.Select(filterInfo => filterInfo.Filter).Any(filter => filter is IAllowAnonymousFilter);

            if (isAuthorized && !allowAnonymous)
            {
                if (operation.Parameters == null)
                {
                    operation.Parameters = new List<OpenApiParameter>();
                }

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Description = "access token",
                    Required = false,
                    Schema = new OpenApiSchema { Type = "string", Default = new OpenApiString("Bearer ") }
                });
            }
        }
    }
}
