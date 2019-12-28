using SocialNetwork.Identity.EfCore;
using SocialNetwork.Identity.Models;
using SocialNetwork.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.Identity.Services
{
    public interface IUserManager<TUser> : IDisposable
        where TUser : UserBase
    {
        ValueTask<IdentityResult> ActivateAsync(TUser user);
        ValueTask<IdentityResult> ChangePassword(TUser user, string currentPassword, string newPassword);
        ValueTask<bool> CheckPasswordAsync(TUser user, string password);
        ValueTask<IdentityResult> CreateUserAsync(TUser user, string password);
        ValueTask<IdentityResult> DeactivateAsync(TUser user);
        ValueTask<TUser> FindAsync(TUser user);
        ValueTask<TUser> FindByEmailAsync(string email);
        ValueTask<TUser> FindByIdAsync(int id);
        ValueTask<TUser> FindByMobileNumberAsync(string mobileNumber);
        ValueTask<TUser> FindByUserNameAsync(string userName);
        ValueTask<IdentityResult> RemoveAsync(TUser user);
    }

    public class UserManager<TUser> : IUserManager<TUser>, IDisposable
        where TUser : UserBase
    {
        private readonly IdentityDbContext<TUser> db;
        private readonly IHasher hasher;
        private readonly ILogger<UserManager<TUser>> logger;

        public UserManager(
            IdentityDbContext<TUser> db,
            IHasher hasher,
            ILogger<UserManager<TUser>> logger)
        {
            this.db = db;
            this.hasher = hasher;
            this.logger = logger;
        }

        public async ValueTask<IdentityResult> CreateUserAsync(TUser user, string password)
        {
            // validate user
            // validate password
            user.Email = user.Email.ToLower();
            user.UserName = user.UserName.ToLower();
            user.PasswordHash = hasher.Hash(password);
            user.IsActive = true;
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();
            return IdentityResult.Success;
        }
        public async ValueTask<bool> CheckPasswordAsync(TUser user, string password)
        {
            if (user == null) return false;
            user = await db.Users.FindAsync(user.Id);
            if (user.PasswordHash == hasher.Hash(password))
                return true;
            logger.LogWarning("Password mismatch for user {0}", user.Id);
            return false;
        }
        public async ValueTask<IdentityResult> ChangePassword(
            TUser user, string currentPassword, string newPassword)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user = await db.Users.FindAsync(user.Id);
            if (user.PasswordHash == hasher.Hash(currentPassword))
            {
                user.PasswordHash = hasher.Hash(newPassword);
                db.Users.Update(user);
                await db.SaveChangesAsync();
                logger.LogInformation("User {0} password changed", user.Id);
                return IdentityResult.Success;
            }
            else
            {
                logger.LogWarning("Change password failed for user {0}", user.Id);
                return IdentityResult.Failed();
            }
        }
        public async ValueTask<IdentityResult> DeactivateAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user = await db.Users.FindAsync(user.Id);
            user.IsActive = false;
            db.Users.Update(user);
            await db.SaveChangesAsync();
            logger.LogInformation("User {0} deactivated");
            return IdentityResult.Success;
        }
        public async ValueTask<IdentityResult> ActivateAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user = await db.Users.FindAsync(user.Id);
            user.IsActive = true;
            db.Users.Update(user);
            await db.SaveChangesAsync();
            logger.LogInformation("User {0} activated");
            return IdentityResult.Success;
        }

        public async ValueTask<IdentityResult> RemoveAsync(TUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user = await db.Users.FindAsync(user.Id);
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            logger.LogInformation("User {0} deleted");
            return IdentityResult.Success;
        }

        public ValueTask<TUser> FindByIdAsync(int id)
            => new ValueTask<TUser>(db.Users.FirstOrDefaultAsync(u => u.Id == id));
        public ValueTask<TUser> FindByUserNameAsync(string userName)
            => new ValueTask<TUser>(db.Users.FirstOrDefaultAsync(u => u.UserName == userName.ToLower()));
        public ValueTask<TUser> FindByEmailAsync(string email)
            => new ValueTask<TUser>(db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower()));
        public ValueTask<TUser> FindByMobileNumberAsync(string mobileNumber)
            => new ValueTask<TUser>(db.Users.FirstOrDefaultAsync(u => u.MobileNumber == mobileNumber));
        public ValueTask<TUser> FindAsync(TUser user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));
            if (user.Id != default)
                return FindByIdAsync(user.Id);
            if (user.UserName != null)
                return FindByUserNameAsync(user.UserName);
            if (user.Email != null)
                return FindByEmailAsync(user.Email);
            if (user.MobileNumber != null)
                return FindByMobileNumberAsync(user.MobileNumber);

            return new ValueTask<TUser>((TUser)null);
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

                // TODO: set large fields to null.
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
