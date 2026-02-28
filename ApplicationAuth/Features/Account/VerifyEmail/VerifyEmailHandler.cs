using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Common.Utilities.Interfaces;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.SharedModels.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ApplicationAuth.Features.Account.VerifyEmail;

public class VerifyEmailHandler : IRequestHandler<VerifyEmailRequest, VerifyEmailResponse>
{
    private readonly IDataContext _dataContext;
    private readonly IHashUtility _hashService;

    public VerifyEmailHandler(IDataContext dataContext, IHashUtility hashService)
    {
        _dataContext = dataContext;
        _hashService = hashService;
    }

    public async Task<VerifyEmailResponse> Handle(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLower();

        var user = await _dataContext.Set<ApplicationUser>()
            .Include(u => u.VerificationTokens)
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email, cancellationToken);

        if (user == null)
            throw new CustomException(HttpStatusCode.NotFound, "email", "User not found");

        if (user.IsActive)
            throw new CustomException(HttpStatusCode.BadRequest, "email", "Account is already active");

        var token = user.VerificationTokens
            .Where(t => t.Type == VerificationCodeType.Confirm && !t.IsUsed && t.CreateDate >= DateTime.UtcNow.AddMinutes(-15))
            .OrderByDescending(t => t.CreateDate)
            .FirstOrDefault();

        if (token == null)
            throw new CustomException(HttpStatusCode.BadRequest, "code", "Invalid or expired verification code.");

        var inputHash = _hashService.GetHash(request.Code);
        bool isCodeValid = inputHash == token.TokenHash;
        
        if (!isCodeValid)
            throw new CustomException(HttpStatusCode.BadRequest, "code", "Incorrect verification code.");

        user.IsActive = true;
        user.EmailConfirmed = true;
        token.IsUsed = true;

        await _dataContext.SaveChangesAsync(cancellationToken);

        return new VerifyEmailResponse(true, "Email has been successfully verified! You can now login.");
    }
}
