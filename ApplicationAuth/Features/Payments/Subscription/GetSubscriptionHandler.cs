using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.SharedModels.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationAuth.Features.Payments.Subscription
{
    public record GetSubscriptionQuery(int UserId) : IRequest<SubscriptionResponse?>;

    public record SubscriptionResponse(
        int Id,
        string PlanName,
        string PlanInterval,
        decimal AmountUsd,
        string Status,
        DateTime? PeriodStart,
        DateTime? PeriodEnd,
        bool CancelAtPeriodEnd
    );

    public class GetSubscriptionHandler : IRequestHandler<GetSubscriptionQuery, SubscriptionResponse?>
    {
        private readonly IDataContext _db;

        public GetSubscriptionHandler(IDataContext db) => _db = db;

        public async Task<SubscriptionResponse?> Handle(GetSubscriptionQuery request, CancellationToken ct)
        {
            var sub = await _db.Set<Domain.Entities.Identity.Subscription>()
                .Include(s => s.Plan)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync(s => s.UserId == request.UserId, ct);

            if (sub == null) return null;

            return new SubscriptionResponse(
                sub.Id,
                sub.Plan.Name,
                sub.Plan.Interval.ToString(),
                sub.Plan.AmountCents / 100m,
                sub.Status.ToString(),
                sub.CurrentPeriodStart,
                sub.CurrentPeriodEnd,
                sub.CancelAtPeriodEnd
            );
        }
    }
}
