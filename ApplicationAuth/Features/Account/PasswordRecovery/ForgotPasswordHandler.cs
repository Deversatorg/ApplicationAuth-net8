using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Common.Utilities.Interfaces;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Features.Account.Shared;
using ApplicationAuth.SharedModels.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationAuth.Features.Account.PasswordRecovery;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordRequest, bool>
{
    private readonly IDataContext _dataContext;
    private readonly IHashUtility _hashUtility;
    private readonly IEmailService _emailService;

    public ForgotPasswordHandler(IDataContext dataContext, IHashUtility hashUtility, IEmailService emailService)
    {
        _dataContext = dataContext;
        _hashUtility = hashUtility;
        _emailService = emailService;
    }

    public async Task<bool> Handle(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await _dataContext.Set<ApplicationUser>()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && !u.IsDeleted, cancellationToken);
            
        if (user != null)
        {
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();
            var hash = _hashUtility.GetHash(code);

            var token = new VerificationToken
            {
                UserId = user.Id,
                TokenHash = hash,
                CreateDate = DateTime.UtcNow,
                IsUsed = false,
                Type = VerificationCodeType.ResetPassword
            };
            
            _dataContext.Set<VerificationToken>().Add(token);
            await _dataContext.SaveChangesAsync(cancellationToken);

            var mailSubject = "ApplicationAuth - Reset Password Code";
            var mailMessage = $"Your password reset code is: {code} \r\nIf you did not request a password reset, please ignore this email. This code will expire in 5 minutes.";
            
            await _emailService.SendEmailAsync(user.Email, mailSubject, mailMessage);
        }

        // Return true regardless of user existence to prevent email enumeration
        return true;
    }
}
