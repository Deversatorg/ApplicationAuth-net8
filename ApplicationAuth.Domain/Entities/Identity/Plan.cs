using ApplicationAuth.SharedModels.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ApplicationAuth.Domain.Entities.Identity
{
    public class Plan : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Stripe Price ID (price_xxx). Empty in dev/mock mode.
        /// </summary>
        [MaxLength(200)]
        public string StripePriceId { get; set; }

        /// <summary>
        /// Price in the smallest currency unit (e.g. cents). $9.99 = 999.
        /// </summary>
        public int AmountCents { get; set; }

        [MaxLength(3)]
        [DefaultValue("usd")]
        public string Currency { get; set; } = "usd";

        public PlanInterval Interval { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
