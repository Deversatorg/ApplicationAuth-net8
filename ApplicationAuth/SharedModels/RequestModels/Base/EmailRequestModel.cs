using ApplicationAuth.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ApplicationAuth.SharedModels.RequestModels
{
    public record EmailRequestModel
    {
        [Required(ErrorMessage = "Email address field is empty")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email address is not in valid format")]
        [StringLength(129, ErrorMessage = "Email address is not in valid format", MinimumLength = 3)]
        [CustomRegularExpression(ModelRegularExpression.REG_EMAIL, ErrorMessage = "Email address is not in valid format")]
        [CustomRegularExpression(ModelRegularExpression.REG_EMAIL_DOMAINS, ErrorMessage = "Email address is not in valid format")]
        public string Email { get; set; }
    }
}
