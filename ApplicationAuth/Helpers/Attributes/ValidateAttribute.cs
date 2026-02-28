using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using ApplicationAuth.SharedModels.ResponseModels;
using ApplicationAuth.ResourceLibrary;
using System;
using System.Linq;

namespace ApplicationAuth.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ValidateAttribute : ActionFilterAttribute
    {
        private IStringLocalizer<ErrorsResource> _errorsLocalizer;
        private ErrorResponseModel _errors;

        public bool IsReusable => throw new NotImplementedException();

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (_errorsLocalizer == null)
                using (var scope = context.HttpContext.RequestServices.CreateScope())
                {
                    _errorsLocalizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<ErrorsResource>>();
                }

            // Apply this logic only if action have one reference type parameter
            if (context.ActionDescriptor.Parameters.Count > 0 && context.ActionDescriptor.Parameters.Any(x => x.ParameterType.IsClass))
            {
                var actionData = context.ActionDescriptor.GetType().GetMethod(context.ActionDescriptor.DisplayName);

                // Return bad request if model is null or non of argument if value type
                if (!context.ActionArguments.Any())
                {
                    _errors = new ErrorResponseModel(_errorsLocalizer);
                    context.Result = _errors.BadRequest("model", "Model is invalid");

                    return;
                }
                else if (context.ActionArguments.Count != context.ActionDescriptor.Parameters.Count)
                {
                    // In case if one of argument empty (it can not be bind) and not optional return error with this model
                    var data = context.ActionDescriptor.Parameters.Where(x => x.ParameterType.IsClass && !((ControllerParameterDescriptor)x).ParameterInfo.IsOptional && !context.ActionArguments.ContainsKey(x.Name)).ToList();

                    if (data.Any())
                    {
                        _errors = new ErrorResponseModel(_errorsLocalizer);
                        foreach (var x in data)
                        {
                            _errors.AddError(x.Name, $"{x.Name} is invalid");
                        }

                        context.Result = _errors.BadRequest();
                        return;
                    }
                }
            }

            // Build validation errors response
            if (!context.ModelState.IsValid)
            {
                _errors = new ErrorResponseModel(_errorsLocalizer);
                _errors.BuildErrors(context.ModelState);
                context.Result = _errors.BadRequest();
            }

            return;
        }
    }
}
