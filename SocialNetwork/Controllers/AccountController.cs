using SocialNetwork.Identity.Services;
using SocialNetwork.Security.Cryptography;
using SocialNetwork.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialNetwork.Data;
using SocialNetwork.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocialNetwork.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace SocialNetwork.Controllers
{
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> logger;
        private readonly IHasher hasher;
        private readonly IJwtAuthManager<User> authManager;
        private readonly AppDbContext db;

        public AccountController(
            ILogger<AccountController> logger,
            IHasher hasher,
            AppDbContext db,
            IJwtAuthManager<User> authManager
            )
        {
            this.hasher = hasher;
            this.db = db;
            this.authManager = authManager;
            this.logger = logger;
        }

        [HttpPost]
        public async ValueTask<IActionResult> SignIn([FromBody]AccountVm account)
        {
            ModelState.Remove(nameof(AccountVm.PasswordConf));
            if (!string.IsNullOrEmpty(account?.Email))
            {
                ModelState.Remove(nameof(AccountVm.Email));
            }
            var user = await db.Users.FirstOrDefaultAsync(u =>
                u.UserName == account.UserName ||
                u.Email == account.Email ||
                u.MobileNumber == account.Mobile);
            if (user is null)
            {
                logger.LogInformation("login request for not existing user");
                ModelState.AddModelError(nameof(AccountVm.UserName), "User Not Found");
            }
            else
            {
                var isPass = user.PasswordHash == hasher.Hash(account.Password);
                if (!isPass)
                {
                    logger.LogInformation("login request failed because of UserName or password mismatch");
                    ModelState.AddModelError(nameof(AccountVm.Password), "UserName or Password mismatch");
                }
                if (!user.IsActive)
                    ModelState.AddModelError(nameof(AccountVm.UserName), "user is deactivated");
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var jwt = await authManager.CreateJwtTokenAsync(user, user.UserName);
            logger.LogInformation("user signed in");
            return Ok(jwt);
        }

        [HttpPost]
        public async ValueTask<IActionResult> SignUp([FromBody]AccountVm account)
        {
            if (account is null) return BadRequest("empty body");
            var user = await db.Users.FirstOrDefaultAsync(u =>
                u.UserName == account.UserName ||
                u.Email == account.Email ||
                u.MobileNumber == account.Mobile);
            if (user != null)
            {
                if (user.UserName == account.UserName)
                    ModelState.AddModelError("UserName", "UserName name had taken");
                if (user.Email == account.Email)
                    ModelState.AddModelError("Email", "Email had taken");
                if (user.MobileNumber == account.Mobile)
                    ModelState.AddModelError("Mobile", "Mobile had taken");
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            user = new User
            {
                UserName = account.UserName.ToLower(),
                Email = account.Email.ToLower(),
                MobileNumber = account.Mobile,
                PasswordHash = hasher.Hash(account.Password),
                IsPrivate = account.IsPrivate,
                IsActive = true,
            };

            await db.AddAsync(user);
            await db.AddAsync(new Follow
            {
                Followed = user,
                Follower = user,
                Accepted = true,
                Time = DateTime.Now,
            });
            db.SaveChanges();
            logger.LogInformation($"user {user.UserName} signed up");
            return Ok();
        }

        [HttpPost, Auth]
        public async ValueTask<IActionResult> Edit([FromBody]AccountVm account)
        {
            if (account is null) return BadRequest("empty body");
            var currentUser = await db.Users.FirstOrDefaultAsync(u =>
                u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));
            var users = db.Users.Where(u =>
                u.UserName == account.UserName ||
                u.Email == account.Email ||
                u.MobileNumber == account.Mobile);
            var user = users.FirstOrDefault();
            if (user != null && user != currentUser)
            {
                if (user.UserName == account.UserName)
                    ModelState.AddModelError("UserName", "UserName name had taken");
                if (user.Email == account.Email)
                    ModelState.AddModelError("Email", "Email had taken");
                if (user.MobileNumber == account.Mobile)
                    ModelState.AddModelError("Mobile", "Mobile had taken");
            }
            if (!string.IsNullOrEmpty(account.OldPassword))
            {
                if (hasher.Hash(account.OldPassword) != currentUser.PasswordHash)
                    ModelState.AddModelError("OldPassword", "Old Password mismatch");

                if (ModelState.IsValid)
                    currentUser.PasswordHash = hasher.Hash(account.Password);
            }
            else
            {
                ModelState.Remove(nameof(AccountVm.OldPassword));
                ModelState.Remove(nameof(AccountVm.Password));
                ModelState.Remove(nameof(AccountVm.PasswordConf));
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            currentUser.Email = account.Email;
            currentUser.MobileNumber = account.Mobile;
            currentUser.IsPrivate = account.IsPrivate;

            db.Update(currentUser);
            db.SaveChanges();
            return Ok();
        }


        [HttpPost, Auth("Admin")]
        public async ValueTask<IActionResult> DeactiveUser([FromBody]AccountVm account)
        {
            if (!ModelState.IsValid || account.UserName is null)
                return BadRequest(ModelState);

            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == account.UserName);
            if (user is null)
                return BadRequest("user is not exist");

            user.IsActive = false;
            db.Update(user);
            await db.SaveChangesAsync();
            logger.LogInformation("user deactivated");
            return Ok();
        }
    }
}
