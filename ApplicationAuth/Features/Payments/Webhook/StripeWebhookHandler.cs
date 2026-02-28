using ApplicationAuth.Common.Constants;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.SharedModels.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ApplicationAuth.Features.Payments.Webhook
{
    /// <summary>
    /// Handles incoming Stripe webhook events and updates subscription state in DB.
    /// </summary>
    public class StripeWebhookHandler
    {
        private readonly IDataContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<StripeWebhookHandler> _logger;

        public StripeWebhookHandler(IDataContext db, IConfiguration config, ILogger<StripeWebhookHandler> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        public async Task HandleAsync(Stream requestBody, string stripeSignature, CancellationToken ct)
        {
            var webhookSecret = _config["Stripe:WebhookSecret"];
            var isMockMode = string.IsNullOrEmpty(_config["Stripe:SecretKey"]);

            Event stripeEvent;

            if (isMockMode)
            {
                _logger.LogWarning("[MOCK] Stripe webhook received but no webhook secret configured. Skipping signature validation.");
                return;
            }

            try
            {
                var json = await new StreamReader(requestBody).ReadToEndAsync(ct);
                stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Stripe webhook signature validation failed");
                throw;
            }

            _logger.LogInformation("Stripe webhook received: {EventType}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case StripeWebhookType.SubscriptionCreated:
                case StripeWebhookType.SubscriptionUpdated:
                    await HandleSubscriptionUpdatedAsync(stripeEvent, ct);
                    break;

                case StripeWebhookType.SubscriptionDeleted:
                    await HandleSubscriptionDeletedAsync(stripeEvent, ct);
                    break;

                case StripeWebhookType.InvoicePaymentSucceeded:
                    await HandleInvoicePaymentSucceededAsync(stripeEvent, ct);
                    break;

                case StripeWebhookType.InvoicePaymentFailed:
                    await HandleInvoicePaymentFailedAsync(stripeEvent, ct);
                    break;

                default:
                    _logger.LogDebug("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                    break;
            }
        }

        private async Task HandleSubscriptionUpdatedAsync(Event evt, CancellationToken ct)
        {
            var stripeSub = evt.Data.Object as Stripe.Subscription;
            if (stripeSub == null) return;

            var sub = await FindOrCreateSubscriptionAsync(stripeSub, ct);
            if (sub == null) return;

            sub.StripeSubscriptionId = stripeSub.Id;
            sub.Status = MapStatus(stripeSub.Status);
            sub.CurrentPeriodStart = stripeSub.CurrentPeriodStart;
            sub.CurrentPeriodEnd = stripeSub.CurrentPeriodEnd;
            sub.CancelAtPeriodEnd = stripeSub.CancelAtPeriodEnd;
            sub.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Subscription {SubId} updated → {Status}", stripeSub.Id, sub.Status);
        }

        private async Task HandleSubscriptionDeletedAsync(Event evt, CancellationToken ct)
        {
            var stripeSub = evt.Data.Object as Stripe.Subscription;
            if (stripeSub == null) return;

            var sub = await _db.Set<Domain.Entities.Identity.Subscription>()
                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSub.Id, ct);

            if (sub == null) return;

            sub.Status = SubscriptionStatus.Canceled;
            sub.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Subscription {SubId} canceled", stripeSub.Id);
        }

        private async Task HandleInvoicePaymentSucceededAsync(Event evt, CancellationToken ct)
        {
            var invoice = evt.Data.Object as Invoice;
            if (invoice?.SubscriptionId == null) return;

            var sub = await _db.Set<Domain.Entities.Identity.Subscription>()
                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == invoice.SubscriptionId, ct);

            if (sub == null) return;

            sub.Status = SubscriptionStatus.Active;
            sub.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Payment succeeded for subscription {SubId}", invoice.SubscriptionId);
        }

        private async Task HandleInvoicePaymentFailedAsync(Event evt, CancellationToken ct)
        {
            var invoice = evt.Data.Object as Invoice;
            if (invoice?.SubscriptionId == null) return;

            var sub = await _db.Set<Domain.Entities.Identity.Subscription>()
                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == invoice.SubscriptionId, ct);

            if (sub == null) return;

            sub.Status = SubscriptionStatus.PastDue;
            sub.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            _logger.LogWarning("Payment FAILED for subscription {SubId}", invoice.SubscriptionId);
        }

        private async Task<Domain.Entities.Identity.Subscription?> FindOrCreateSubscriptionAsync(Stripe.Subscription stripeSub, CancellationToken ct)
        {
            // Try to match by Stripe subscription ID first
            var sub = await _db.Set<Domain.Entities.Identity.Subscription>()
                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSub.Id, ct);

            if (sub != null) return sub;

            // Try to match by Stripe customer ID (pending subscription created at checkout)
            sub = await _db.Set<Domain.Entities.Identity.Subscription>()
                .Where(s => s.StripeCustomerId == stripeSub.CustomerId && s.Status == SubscriptionStatus.Incomplete)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync(ct);

            return sub;
        }

        private static SubscriptionStatus MapStatus(string stripeStatus) => stripeStatus switch
        {
            "active" => SubscriptionStatus.Active,
            "trialing" => SubscriptionStatus.Trialing,
            "past_due" => SubscriptionStatus.PastDue,
            "canceled" => SubscriptionStatus.Canceled,
            "unpaid" => SubscriptionStatus.Unpaid,
            _ => SubscriptionStatus.Incomplete
        };
    }
}
