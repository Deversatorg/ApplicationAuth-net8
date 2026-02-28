using ApplicationAuth.SharedModels.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationAuth.Domain.Entities.Identity
{
    public class Subscription : IEntity
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        /// <summary>
        /// Stripe Customer ID (cus_xxx). Empty in mock mode.
        /// </summary>
        [MaxLength(200)]
        public string StripeCustomerId { get; set; }

        /// <summary>
        /// Stripe Subscription ID (sub_xxx). Empty in mock mode.
        /// </summary>
        [MaxLength(200)]
        public string StripeSubscriptionId { get; set; }

        public int PlanId { get; set; }

        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Incomplete;

        public DateTime? CurrentPeriodStart { get; set; }

        public DateTime? CurrentPeriodEnd { get; set; }

        /// <summary>
        /// Whether the subscription cancels at the end of current period.
        /// </summary>
        public bool CancelAtPeriodEnd { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("UserId")]
        [InverseProperty("Subscriptions")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("PlanId")]
        public virtual Plan Plan { get; set; }
    }
}
