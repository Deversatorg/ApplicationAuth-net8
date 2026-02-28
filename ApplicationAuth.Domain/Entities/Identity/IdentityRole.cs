using Microsoft.AspNetCore.Identity;
using ApplicationAuth.Common.Extensions;
using System.Collections.Generic;

namespace ApplicationAuth.Domain.Entities.Identity
{
    public class ApplicationRole : IdentityRole<int>, IEntity
    {
        public override int Id { get; set; }

        public ICollection<ApplicationUserRole> UserRoles { get; set; }

        public ApplicationRole()
        {
            UserRoles = UserRoles.Empty();
        }
    }
}
