using Gihan.Identity.Models;

using Microsoft.EntityFrameworkCore;

namespace Gihan.Identity.EfCore;

public class IdentityDbContext<TUser, TAccessGroupMembership, TAccessToken>(DbContextOptions options) : DbContext(options)
    where TUser : UserBase
    where TAccessGroupMembership : UserRole
    where TAccessToken : AccessToken
{
    public DbSet<Role> Roles { get; set; }
    public DbSet<TAccessGroupMembership> UserRoles { get; set; }
    public DbSet<TAccessToken> AccessTokens { get; set; }
    public DbSet<TUser> Users { get; set; }
}

public class IdentityDbContext<TUser>(DbContextOptions options) : IdentityDbContext<TUser, UserRole, AccessToken>(options)
    where TUser : UserBase
{
}
