using System;
using System.ComponentModel.DataAnnotations;

namespace ApplicationAuth.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateLaterThanAttribute : ValidationAttribute
    {
        private string _dateToCompareFieldName { get; set; }

        public DateLaterThanAttribute(string dateToCompareFieldName)
        {
            _dateToCompareFieldName = dateToCompareFieldName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var laterDate = value as DateTime?;

            var earlierDate = validationContext.ObjectType.GetProperty(_dateToCompareFieldName).GetValue(validationContext.ObjectInstance, null) as DateTime?;

            if (!earlierDate.HasValue || !laterDate.HasValue || laterDate >= earlierDate)
                return ValidationResult.Success;
            else
                return new ValidationResult("Date must be later or equal to compared value");
        }
    }
}
