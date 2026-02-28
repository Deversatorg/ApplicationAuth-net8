using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Common.Constants;
using ApplicationAuth.Common.Utilities.Interfaces;
using ApplicationAuth.Features.Account.Shared;
using ApplicationAuth.SharedModels.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;

using System.Threading;
using MediatR;

namespace ApplicationAuth.Features.Account.Register;

public class RegisterHandler : IRequestHandler<RegisterRequest, RegisterResponse>
{
    private readonly IDataContext _dataContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IHashUtility _hashService;

    public RegisterHandler(IDataContext dataContext, UserManager<ApplicationUser> userManager, IEmailService emailService, IHashUtility hashService)
    {
        _dataContext = dataContext;
        _userManager = userManager;
        _emailService = emailService;
        _hashService = hashService;
    }

    public async Task<RegisterResponse> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLower();

        var existingUser = _dataContext.Set<ApplicationUser>().FirstOrDefault(x => x.Email.ToLower() == email);

        if (existingUser != null && existingUser.EmailConfirmed)
            throw new CustomException(HttpStatusCode.UnprocessableEntity, "email", "Email is already registered");

        ApplicationUser user = existingUser;

        if (user == null)
        {
            user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                IsActive = false,
                RegistratedAt = DateTime.UtcNow,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                throw new CustomException(HttpStatusCode.BadRequest, "general", result.Errors.FirstOrDefault()?.Description ?? "Failed to create user");

            result = await _userManager.AddToRoleAsync(user, Role.User);

            if (!result.Succeeded)
                throw new CustomException(HttpStatusCode.BadRequest, "general", result.Errors.FirstOrDefault()?.Description ?? "Failed to assign role");
        }

        // Generate a 6-digit confirmation code
        var random = new Random();
        var code = random.Next(100000, 999999).ToString();
        var codeHash = _hashService.GetHash(code);

        _dataContext.Set<VerificationToken>().Add(new VerificationToken
        {
            UserId = user.Id,
            Type = VerificationCodeType.Confirm,
            CreateDate = DateTime.UtcNow,
            TokenHash = codeHash,
            IsUsed = false
        });
        await _dataContext.SaveChangesAsync(cancellationToken);

        string mailSubject = "Confirm your email address";
        string mailMessage = $"Your confirmation code is: {code}";
        await _emailService.SendEmailAsync(user.Email, mailSubject, mailMessage);

        return new RegisterResponse(Email: user.Email);
    }
}
