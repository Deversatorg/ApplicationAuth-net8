using System;
using System.ComponentModel.DataAnnotations;

namespace ApplicationAuth.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ValidateIdAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var id = value as int?;

            if (id > 0)
                return ValidationResult.Success;
            else
                return new ValidationResult("Invalid Id");
        }
    }
}
