using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.Domain.Entities.Identity;
using System.Net;

namespace ApplicationAuth.Domain.Extentions
{
    public static class ApplicationUserExtensions
    {
        public static void ThrowIfBlocked(this ApplicationUser user)
        {
            if (!user.IsActive)
                throw new CustomException(HttpStatusCode.Forbidden, "general", "Your account has been blocked. For more information please email administrator at: ");
        }
    }
}
