using SocialNetwork.Identity.Models;
using SocialNetwork.EfCore;
using Microsoft.EntityFrameworkCore;

namespace SocialNetwork.Identity.EfCore
{
    public class IdentityDbContext<TUser, TAccessGroupMembership, TAccessToken> : DbContext
        where TUser : UserBase
        where TAccessGroupMembership : UserRole
        where TAccessToken : AccessToken
    {
        public IdentityDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Role> Roles { get; set; }
        public DbSet<TAccessGroupMembership> UserRoles { get; set; }
        public DbSet<TAccessToken> AccessTokens { get; set; }
        public DbSet<TUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseGsnDataAnnotations();
            base.OnModelCreating(modelBuilder);
        }
    }

    public class IdentityDbContext<TUser>
        : IdentityDbContext<TUser, UserRole, AccessToken>
        where TUser : UserBase
    {
        public IdentityDbContext(DbContextOptions options) : base(options)
        {
        }
    }

    //public class IdentityDbContext : IdentityDbContext<UserBase>
    //{
    //    public IdentityDbContext(DbContextOptions options) : base(options)
    //    {
    //    }
    //}
}
