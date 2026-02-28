using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using System.Threading.Tasks;

namespace ApplicationAuth.Features.Payments.Shared
{
    /// <summary>
    /// Real Stripe SDK implementation. Used when Stripe:SecretKey is set in config.
    /// </summary>
    public class StripeService : IStripeService
    {
        public StripeService(IConfiguration configuration)
        {
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
        }

        public async Task<string> CreateOrGetCustomerAsync(string email, string existingCustomerId = null)
        {
            if (!string.IsNullOrEmpty(existingCustomerId))
                return existingCustomerId;

            var service = new CustomerService();
            var customer = await service.CreateAsync(new CustomerCreateOptions { Email = email });
            return customer.Id;
        }

        public async Task<string> CreateCheckoutSessionAsync(string customerId, string priceId, string successUrl, string cancelUrl, int userId)
        {
            var service = new SessionService();
            var session = await service.CreateAsync(new SessionCreateOptions
            {
                Customer = customerId,
                PaymentMethodTypes = new() { "card" },
                LineItems = new()
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1
                    }
                },
                Mode = "subscription",
                SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = cancelUrl,
                Metadata = new() { ["userId"] = userId.ToString() }
            });
            return session.Url;
        }

        public async Task<StripeSubscriptionInfo> GetSubscriptionAsync(string subscriptionId)
        {
            var service = new SubscriptionService();
            var sub = await service.GetAsync(subscriptionId);
            return new StripeSubscriptionInfo(
                sub.Id,
                sub.Status,
                new DateTimeOffset(sub.CurrentPeriodStart, TimeSpan.Zero).ToUnixTimeSeconds(),
                new DateTimeOffset(sub.CurrentPeriodEnd, TimeSpan.Zero).ToUnixTimeSeconds(),
                sub.CancelAtPeriodEnd
            );
        }

        public async Task CancelSubscriptionAsync(string subscriptionId)
        {
            var service = new SubscriptionService();
            await service.UpdateAsync(subscriptionId, new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = true
            });
        }
    }
}
