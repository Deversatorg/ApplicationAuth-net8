using System;
using System.Threading.Tasks;

namespace ApplicationAuth.Features.Payments.Shared
{
    /// <summary>
    /// Mock Stripe service for development/testing when no Stripe API key is configured.
    /// Returns realistic-looking deterministic data without making any real API calls.
    /// </summary>
    public class MockStripeService : IStripeService
    {
        public Task<string> CreateOrGetCustomerAsync(string email, string existingCustomerId = null)
        {
            if (!string.IsNullOrEmpty(existingCustomerId))
                return Task.FromResult(existingCustomerId);

            // Deterministic mock customer ID based on email hash
            var mockId = $"cus_MOCK_{Math.Abs(email.GetHashCode()):X8}";
            return Task.FromResult(mockId);
        }

        public Task<string> CreateCheckoutSessionAsync(string customerId, string priceId, string successUrl, string cancelUrl, int userId)
        {
            // Returns a dummy URL that looks like a real Stripe checkout link
            var sessionId = $"cs_test_MOCK_{Guid.NewGuid():N}";
            var mockUrl = $"https://checkout.stripe.com/c/pay/{sessionId}#mock-dev-mode";
            return Task.FromResult(mockUrl);
        }

        public Task<StripeSubscriptionInfo> GetSubscriptionAsync(string subscriptionId)
        {
            var now = DateTimeOffset.UtcNow;
            return Task.FromResult(new StripeSubscriptionInfo(
                Id: subscriptionId,
                Status: "active",
                PeriodStart: now.ToUnixTimeSeconds(),
                PeriodEnd: now.AddMonths(1).ToUnixTimeSeconds(),
                CancelAtPeriodEnd: false
            ));
        }

        public Task CancelSubscriptionAsync(string subscriptionId)
        {
            // No-op in mock mode — cancellation is handled in the DB directly
            return Task.CompletedTask;
        }
    }
}
