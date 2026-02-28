using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationAuth.Features.Payments.Plans
{
    public record GetPlansQuery : IRequest<List<PlanResponse>>;

    public record PlanResponse(
        int Id,
        string Name,
        string Description,
        decimal AmountUsd,
        string Currency,
        string Interval,
        bool IsActive
    );

    public class GetPlansHandler : IRequestHandler<GetPlansQuery, List<PlanResponse>>
    {
        private readonly IDataContext _db;

        public GetPlansHandler(IDataContext db) => _db = db;

        public async Task<List<PlanResponse>> Handle(GetPlansQuery request, CancellationToken ct)
        {
            return await _db.Set<Plan>()
                .Where(p => p.IsActive)
                .OrderBy(p => p.AmountCents)
                .Select(p => new PlanResponse(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.AmountCents / 100m,
                    p.Currency.ToUpper(),
                    p.Interval.ToString(),
                    p.IsActive
                ))
                .ToListAsync(ct);
        }
    }
}
