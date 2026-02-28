using System.ComponentModel.DataAnnotations;

namespace ApplicationAuth.SharedModels.RequestModels
{
    public record BaseIdRequestModel
    {
        [Required(ErrorMessage = "{0} is required")]
        [Range(1, int.MaxValue, ErrorMessage = "{0} is invalid")]
        public int? Id { get; set; }
    }
}
