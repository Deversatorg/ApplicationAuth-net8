using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationAuth.Domain.Entities.Identity
{
    public class UserToken: IEntity
    {
        #region Properties

        public int Id { get; set; }

        public int UserId { get; set; }

        [MaxLength(200)]
        public string AccessTokenHash { get; set; }

        [MaxLength(200)]
        public string RefreshTokenHash { get; set; }

        public DateTime AccessExpiresDate { get; set; }

        public DateTime RefreshExpiresDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }

        #endregion

        #region Navigation Properties

        [ForeignKey("UserId")]
        [InverseProperty("Tokens")]
        public virtual ApplicationUser User { get; set; }
        
        #endregion
    }
}
