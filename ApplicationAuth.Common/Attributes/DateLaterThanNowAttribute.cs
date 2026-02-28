using System;
using System.ComponentModel.DataAnnotations;

namespace ApplicationAuth.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateLaterThanNowAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var laterDate = value as DateTime?;

            if (!laterDate.HasValue || laterDate >= DateTime.UtcNow)
                return ValidationResult.Success;
            else
                return new ValidationResult("Date must be in future");
        }
    }
}
