using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Gihan.Identity;
using Gihan.Identity.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SocialNetwork.Data;
using SocialNetwork.Models;
using SocialNetwork.Models.ViewModels;

namespace SocialNetwork.Controllers;

public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IJwtAuthManager<User> _authManager;
    private readonly AppDbContext _db;

    public AccountController(
        ILogger<AccountController> logger,
        IPasswordHasher<User> hasher,
        AppDbContext db,
        IJwtAuthManager<User> authManager
        )
    {
        _hasher = hasher;
        _db = db;
        _authManager = authManager;
        _logger = logger;
    }

    [HttpPost]
    public async ValueTask<IActionResult> SignIn([FromBody]AccountVm account)
    {
        ModelState.Remove(nameof(AccountVm.PasswordConf));
        if (!string.IsNullOrEmpty(account?.Email))
        {
            ModelState.Remove(nameof(AccountVm.Email));
        }

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.UserName == account.UserName ||
            u.Email == account.Email ||
            u.MobileNumber == account.Mobile);
        if (user is null)
        {
            _logger.LogInformation("login request for not existing user");
            ModelState.AddModelError(nameof(AccountVm.UserName), "User Not Found");
        }
        else
        {
            var isPass = _hasher.VerifyHashedPassword(user, user.PasswordHash, account.Password) != PasswordVerificationResult.Failed;
            if (!isPass)
            {
                _logger.LogInformation("login request failed because of UserName or password mismatch");
                ModelState.AddModelError(nameof(AccountVm.Password), "UserName or Password mismatch");
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(nameof(AccountVm.UserName), "user is deactivated");
            }
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var jwt = await _authManager.CreateJwtTokenAsync(user, user.UserName);
        _logger.LogInformation("user signed in");
        return Ok(jwt);
    }

    [HttpPost]
    public async ValueTask<IActionResult> SignUp([FromBody]AccountVm account)
    {
        if (account is null)
        {
            return BadRequest("empty body");
        }

        var users = _db.Users.Where(u =>
            u.UserName == account.UserName ||
            u.Email == account.Email ||
            u.MobileNumber == account.Mobile);
        if (users.Any())
        {
            if (users.Any(u => u.UserName == account.UserName))
            {
                ModelState.AddModelError("UserName", "UserName name had taken");
            }

            if (users.Any(u => u.Email == account.Email))
            {
                ModelState.AddModelError("Email", "Email had taken");
            }

            if (users.Any(u => u.MobileNumber == account.Mobile))
            {
                ModelState.AddModelError("Mobile", "Mobile had taken");
            }
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new User
        {
            UserName = account.UserName.ToLower(),
            Email = account.Email.ToLower(),
            MobileNumber = account.Mobile,
            IsPrivate = account.IsPrivate,
            IsActive = true,
        };

        user.PasswordHash = _hasher.HashPassword(user, account.Password);

        await _db.AddAsync(user);
        await _db.AddAsync(new Follow
        {
            Followed = user,
            Follower = user,
            Accepted = true,
            Time = DateTime.Now,
        });
        _db.SaveChanges();
        _logger.LogInformation("user {username} signed up", user.UserName);
        return Ok();
    }

    [HttpPost, Auth]
    public async ValueTask<IActionResult> Edit([FromBody]AccountVm account)
    {
        if (account is null)
        {
            return BadRequest("empty body");
        }

        var currentUser = await _db.Users.FirstOrDefaultAsync(u =>
            u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));
        var users = _db.Users.Where(u =>
            (u.UserName == account.UserName ||
            u.Email == account.Email ||
            u.MobileNumber == account.Mobile) && u != currentUser);
        if (users.Any())
        {
            if (users.Any(u => u.UserName == account.UserName))
            {
                ModelState.AddModelError("UserName", "UserName name had taken");
            }

            if (users.Any(u => u.Email == account.Email))
            {
                ModelState.AddModelError("Email", "Email had taken");
            }

            if (users.Any(u => u.MobileNumber == account.Mobile))
            {
                ModelState.AddModelError("Mobile", "Mobile had taken");
            }
        }

        if (!string.IsNullOrEmpty(account.OldPassword))
        {
            if (_hasher.VerifyHashedPassword(currentUser, currentUser.PasswordHash, account.OldPassword) != PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("OldPassword", "Old Password mismatch");
            }

            if (ModelState.IsValid)
            {
                currentUser.PasswordHash = _hasher.HashPassword(currentUser, account.Password);
            }
        }
        else
        {
            ModelState.Remove(nameof(AccountVm.OldPassword));
            ModelState.Remove(nameof(AccountVm.Password));
            ModelState.Remove(nameof(AccountVm.PasswordConf));
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        currentUser.Email = account.Email;
        currentUser.MobileNumber = account.Mobile;
        currentUser.IsPrivate = account.IsPrivate;

        _db.Update(currentUser);
        await _db.SaveChangesAsync();
        return Ok();
    }

    //[HttpPost, Auth("Admin")]
    //public async ValueTask<IActionResult> DeactiveUser([FromBody]AccountVm account)
    //{
    //    if (!ModelState.IsValid || account.UserName is null)
    //        return BadRequest(ModelState);

    //    var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == account.UserName);
    //    if (user is null)
    //        return BadRequest("user is not exist");

    //    user.IsActive = false;
    //    db.Update(user);
    //    await db.SaveChangesAsync();
    //    logger.LogInformation("user deactivated");
    //    return Ok();
    //}
}
