using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.Common.Utilities.Interfaces;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Features.Account.Shared;
using ApplicationAuth.SharedModels.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationAuth.Features.Account.PasswordRecovery;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordRequest, bool>
{
    private readonly IDataContext _dataContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHashUtility _hashUtility;

    public ResetPasswordHandler(IDataContext dataContext, UserManager<ApplicationUser> userManager, IHashUtility hashUtility)
    {
        _dataContext = dataContext;
        _userManager = userManager;
        _hashUtility = hashUtility;
    }

    public async Task<bool> Handle(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        // Load user via UserManager to avoid EF tracking conflicts when calling ResetPasswordAsync later
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null || user.IsDeleted)
            throw new CustomException(HttpStatusCode.BadRequest, "email", "Invalid email or verification code");

        var activeTokens = await _dataContext.Set<VerificationToken>()
            .Where(t => t.UserId == user.Id && t.Type == VerificationCodeType.ResetPassword && !t.IsUsed)
            .ToListAsync(cancellationToken);

        var inputHash = _hashUtility.GetHash(request.Code);
        var validToken = activeTokens.FirstOrDefault(t => t.TokenHash == inputHash && t.IsValid);

        if (validToken == null)
            throw new CustomException(HttpStatusCode.BadRequest, "code", "Invalid or expired verification code");

        // Mark token as used
        validToken.IsUsed = true;
        await _dataContext.SaveChangesAsync(cancellationToken);

        // Reset the password via UserManager
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

        if (!resetResult.Succeeded)
        {
            var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
            throw new CustomException(HttpStatusCode.BadRequest, "password", $"Failed to reset password. {errors}");
        }

        return true;
    }
}
