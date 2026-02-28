using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationAuth.Domain.Entities.Identity
{
    public class Profile : IEntity<int>
    {
        #region Properties

        // Profile has same id as User
        [ForeignKey("User")]
        public int Id { get; set; }

        [MaxLength(30)]
        public string FirstName { get; set; }

        [MaxLength(30)]
        public string LastName { get; set; }

        public int? AvatarId { get; set; }

        public string BraintreeCustomerId { get; set; }

        public string StripeCustomerId { get; set; }

        #endregion

        #region Navigation properties

        [InverseProperty("Profile")]
        public virtual ApplicationUser User { get; set; }

        #endregion

        #region Additional Properties

        [NotMapped]
        public string FullName
        {
            get
            {
                if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
                    return $"{FirstName} {LastName}";
                else
                    return string.Empty;
            }
        }

        #endregion
    }
}
