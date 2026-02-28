using ApplicationAuth.SharedModels.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationAuth.Domain.Entities.Identity
{
    public class VerificationToken : IEntity
    {
        #region Properties

        public int Id { get; set; }

        public int? UserId { get; set; }

        public string PhoneNumber { get; set; }

        [MaxLength(200)]
        public string TokenHash { get; set; }

        public DateTime CreateDate { get; set; }

        public bool IsUsed { get; set; }

        public VerificationCodeType Type { get; set; }

        /// <summary>
        /// Additional data in Json Format
        /// </summary>
        public string Data { get; set; }

        #endregion

        #region Navigation Properties

        [ForeignKey("UserId")]
        [InverseProperty("VerificationTokens")]
        public virtual ApplicationUser User { get; set; }

        #endregion

        #region Additional Properties

        [NotMapped]
        public bool IsValid
        {
            get
            {
                return (DateTime.UtcNow - CreateDate).TotalMinutes < 15;
            }
        }

        #endregion
    }

}
