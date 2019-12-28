using SocialNetwork.Identity.EfCore;
using SocialNetwork.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SocialNetwork.Identity.Services
{
    public interface IAccessGroupManager<TUser> : IDisposable
        where TUser : UserBase
    {
        ValueTask<IdentityResult> CreateAsync(Role group);
        ValueTask<IdentityResult> DeleteAsync(Role group);
        ValueTask<Role> FindAsync(Role group);
        ValueTask<Role> FindByIdAsync(int accessGroupId);
        ValueTask<Role> FindByNameAsync(string name);
        ValueTask<bool> IsMember(Role group, TUser user);
    }

    public class AccessGroupManager<TUser> : IAccessGroupManager<TUser> where TUser : UserBase
    {
        private readonly ILogger<AccessGroupManager<TUser>> logger;
        private readonly IdentityDbContext<TUser> db;
        private readonly IUserManager<TUser> userManager;

        public AccessGroupManager(
            ILogger<AccessGroupManager<TUser>> logger,
            IdentityDbContext<TUser> db,
            IUserManager<TUser> userManager)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
        }

        public async ValueTask<IdentityResult> CreateAsync(Role group)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }
            // validate group
            group.Name = group.Name.ToLower();
            await db.Roles.AddAsync(group);
            await db.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async ValueTask<IdentityResult> DeleteAsync(Role group)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }
            db.Roles.Remove(group);
            await db.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async ValueTask<Role> FindByIdAsync(int accessGroupId)
        {
            var group = await db.Roles.FindAsync(accessGroupId);
            return group;
        }
        public async ValueTask<Role> FindByNameAsync(string name)
        {
            var group = await db.Roles.FirstOrDefaultAsync(g => g.Name == name);
            return group;
        }

        public async ValueTask<Role> FindAsync(Role group)
        {
            Role resultGroup = null;
            if (resultGroup == null && group.Id != default)
                resultGroup = await db.Roles.FindAsync(group.Id);
            if (resultGroup == null && group.Name != null)
                resultGroup = await db.Roles.FirstOrDefaultAsync(g => g.Name == group.Name);
            return resultGroup;
        }

        public async ValueTask<bool> IsMember(Role group, TUser user)
        {
            return await db.UserRoles.AnyAsync(memebership =>
                group.Id != default ? memebership.RoleId == group.Id :
                group.Name != null ? memebership.Role.Name == group.Name : false
                &&
                user.Id != default ? memebership.UserId == user.Id :
                user.UserName != null ? memebership.User.UserName == user.UserName :
                user.Email != null ? memebership.User.Email == user.Email :
                user.MobileNumber != null ?
                memebership.User.MobileNumber == user.MobileNumber : false);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AccessGroupManager()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
