using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Features.Payments.Shared;
using ApplicationAuth.SharedModels.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationAuth.Features.Payments.Subscription
{
    public record CancelSubscriptionRequest(int UserId) : IRequest<bool>;

    public class CancelSubscriptionHandler : IRequestHandler<CancelSubscriptionRequest, bool>
    {
        private readonly IDataContext _db;
        private readonly IStripeService _stripe;

        public CancelSubscriptionHandler(IDataContext db, IStripeService stripe)
        {
            _db = db;
            _stripe = stripe;
        }

        public async Task<bool> Handle(CancelSubscriptionRequest request, CancellationToken ct)
        {
            var sub = await _db.Set<Domain.Entities.Identity.Subscription>()
                .Where(s => s.UserId == request.UserId && s.Status == SubscriptionStatus.Active)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync(ct)
                ?? throw new CustomException(HttpStatusCode.NotFound, "subscription", "No active subscription found");

            // Cancel on Stripe side (cancels at period end, not immediately)
            if (!string.IsNullOrEmpty(sub.StripeSubscriptionId))
                await _stripe.CancelSubscriptionAsync(sub.StripeSubscriptionId);

            // In mock mode, update the DB directly
            sub.CancelAtPeriodEnd = true;
            sub.UpdatedAt = DateTime.UtcNow;

            // If no real Stripe sub ID, immediately set to Canceled
            if (string.IsNullOrEmpty(sub.StripeSubscriptionId))
                sub.Status = SubscriptionStatus.Canceled;

            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
