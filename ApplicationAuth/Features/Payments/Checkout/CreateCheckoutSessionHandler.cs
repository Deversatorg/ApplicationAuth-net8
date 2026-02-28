using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Features.Payments.Shared;
using ApplicationAuth.SharedModels.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EntitySubscription = ApplicationAuth.Domain.Entities.Identity.Subscription;

namespace ApplicationAuth.Features.Payments.Checkout
{
    public record CreateCheckoutSessionRequest(int UserId, int PlanId) : IRequest<CheckoutSessionResponse>;

    public record CheckoutSessionResponse(string SessionUrl, bool IsMockMode);

    public class CreateCheckoutSessionValidator : AbstractValidator<CreateCheckoutSessionRequest>
    {
        public CreateCheckoutSessionValidator()
        {
            RuleFor(x => x.PlanId).GreaterThan(0).WithMessage("Plan ID must be valid");
        }
    }

    public class CreateCheckoutSessionHandler : IRequestHandler<CreateCheckoutSessionRequest, CheckoutSessionResponse>
    {
        private readonly IDataContext _db;
        private readonly IStripeService _stripe;
        private readonly IConfiguration _config;

        public CreateCheckoutSessionHandler(IDataContext db, IStripeService stripe, IConfiguration config)
        {
            _db = db;
            _stripe = stripe;
            _config = config;
        }

        public async Task<CheckoutSessionResponse> Handle(CreateCheckoutSessionRequest request, CancellationToken ct)
        {
            var plan = await _db.Set<Plan>()
                .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.IsActive, ct)
                ?? throw new CustomException(HttpStatusCode.NotFound, "plan", "Plan not found or is no longer available");

            var user = await _db.Set<ApplicationUser>()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted && u.IsActive, ct)
                ?? throw new CustomException(HttpStatusCode.NotFound, "user", "User not found");

            // Get or create existing Stripe subscription for this user+plan
            var existing = await _db.Set<EntitySubscription>()
                .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.Status == SubscriptionStatus.Active, ct);

            if (existing != null)
                throw new CustomException(HttpStatusCode.Conflict, "subscription", "You already have an active subscription");

            var successUrl = _config["Stripe:SuccessUrl"] ?? "http://localhost:1310/payment-success";
            var cancelUrl  = _config["Stripe:CancelUrl"]  ?? "http://localhost:1310/payment-cancel";

            // Get existing Stripe customer ID if user subscribed before
            var previousSub = await _db.Set<EntitySubscription>()
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync(s => s.UserId == request.UserId, ct);

            var customerId = await _stripe.CreateOrGetCustomerAsync(user.Email, previousSub?.StripeCustomerId);

            var sessionUrl = await _stripe.CreateCheckoutSessionAsync(
                customerId,
                plan.StripePriceId ?? $"mock_price_{plan.Id}",
                successUrl,
                cancelUrl,
                request.UserId
            );

            // Persist a pending subscription record
            var subscription = new EntitySubscription
            {
                UserId = request.UserId,
                PlanId = plan.Id,
                StripeCustomerId = customerId,
                StripeSubscriptionId = string.Empty,
                Status = SubscriptionStatus.Incomplete,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Set<EntitySubscription>().Add(subscription);
            await _db.SaveChangesAsync(ct);

            var isMock = string.IsNullOrEmpty(_config["Stripe:SecretKey"]);
            return new CheckoutSessionResponse(sessionUrl, isMock);
        }
    }
}
