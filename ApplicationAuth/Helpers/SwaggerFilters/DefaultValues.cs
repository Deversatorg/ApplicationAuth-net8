using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace ApplicationAuth.Helpers.SwaggerFilters
{

    /// <summary>
    /// Represents the Swagger/Swashbuckle operation filter used to document the implicit API version parameter.
    /// </summary>
    /// <remarks>This <see cref="IOperationFilter"/> is only required due to bugs in the <see cref="SwaggerGenerator"/>.
    /// Once they are fixed and published, this class can be removed.</remarks>
    public class DefaultValues : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the specified operation using the given context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.</param>
        /// <param name="context">The current operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                return;

            foreach (var parameter in operation.Parameters.OfType<OpenApiParameter>())
            {
                var description = context.ApiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

                var routeInfo = description.RouteInfo;

                if (parameter.Description == null)
                    parameter.Description = description.ModelMetadata?.Description;

                if (parameter.Name.ToLower() == "limit")
                    parameter.Required = true;

                if (routeInfo == null)
                    continue;

                if (parameter.Schema.Default == null && routeInfo.DefaultValue != null)
                    parameter.Schema.Default = new OpenApiString(routeInfo.DefaultValue.ToString());

                if (parameter.Name.ToLower() != "limit")
                    parameter.Required |= !routeInfo.IsOptional;
            }
        }
    }
}
