using System.Threading.Tasks;

namespace ApplicationAuth.Features.Payments.Shared
{
    /// <summary>
    /// Abstraction over Stripe SDK. Swap implementations in DI for real vs mock.
    /// </summary>
    public interface IStripeService
    {
        /// <summary>Creates or retrieves a Stripe Customer for the user.</summary>
        Task<string> CreateOrGetCustomerAsync(string email, string existingCustomerId = null);

        /// <summary>Creates a Stripe Checkout Session and returns the hosted page URL.</summary>
        Task<string> CreateCheckoutSessionAsync(string customerId, string priceId, string successUrl, string cancelUrl, int userId);

        /// <summary>Retrieves a Stripe Subscription by ID.</summary>
        Task<StripeSubscriptionInfo> GetSubscriptionAsync(string subscriptionId);

        /// <summary>Cancels a Stripe Subscription at end of billing period.</summary>
        Task CancelSubscriptionAsync(string subscriptionId);
    }

    public record StripeSubscriptionInfo(
        string Id,
        string Status,
        long? PeriodStart,
        long? PeriodEnd,
        bool CancelAtPeriodEnd
    );
}
