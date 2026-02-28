using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Domain.Entities.Telegram;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ApplicationAuth.DAL
{
    public class DataContext : IdentityDbContext<ApplicationUser, ApplicationRole, int, IdentityUserClaim<int>, ApplicationUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>, IDataContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public virtual DbSet<UserToken> AppUserTokens { get; set; }
        public virtual DbSet<VerificationToken> VerificationTokens { get; set; }
        public virtual DbSet<UserDevice> UserDevices { get; set; }
        public virtual DbSet<ApplicationUserRole> AspNetUserRoles { get; set; }
        public virtual DbSet<UserChangeRequest> UserChangeRequests { get; set; }
        public virtual DbSet<Plan> Plans { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }

        public virtual DbSet<TelegramMessage> TelegramMessages { get; set; }
        public virtual DbSet<TelegramSticker> TelegramStickers { get; set; }

        #region DbSet for stored procedures

        #endregion

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }

        #region Fluent API

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<UserToken>().ToTable("UserTokens");

            modelBuilder.Entity<ApplicationUserRole>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                userRole.HasOne(ur => ur.User)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });
        }

        #endregion

    }
}
