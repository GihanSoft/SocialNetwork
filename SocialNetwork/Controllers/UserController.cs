using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialNetwork.Data;
using SocialNetwork.Identity;
using SocialNetwork.Models;
using SocialNetwork.Models.ViewModels;
using SocialNetwork.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SocialNetwork.Controllers
{
    public class UserController : ControllerBase
    {
        private readonly ILogger<HomeController> logger;
        private readonly AppDbContext db;
        private readonly IHasher hasher;

        public UserController(
            ILogger<HomeController> logger,
            AppDbContext db,
            IHasher hasher
            )
        {
            this.logger = logger;
            this.db = db;
            this.hasher = hasher;
        }

        [HttpPost("API/User/{id}")]
        public async ValueTask<IActionResult> View(string id)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == id);
            if (user == null)
                return BadRequest("User not found");
            var currentUser = await db.Users.FirstOrDefaultAsync(u =>
                u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));
            var haveAccessToPrivateData = User.IsInRole("Admin") || user == currentUser;
            var userVm = new UserVm
            {
                Id = user.Id,
                UserName = user.UserName,
                PostsUrl = Url.Action(nameof(PostController.UserPosts), "Post", new { id }),
                IsFollowed = currentUser?.FollowedFollows?.Select(ff => ff.Followed)?
                    .Any(u => u.Id == user.Id) ?? false,
                FollowersCount = user.FollowerFollows.Count - 1,
                FollowingsCount = user.FollowedFollows.Count - 1,
                Email = haveAccessToPrivateData ? user.Email : null,
                Mobile = haveAccessToPrivateData ? user.MobileNumber : null,
                IsPrivate = user.IsPrivate
            };
            return Ok(userVm);
        }

        [HttpPost, Auth]
        public async ValueTask<IActionResult> Edit([FromBody] AccountVm account)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = await db.Users.FirstOrDefaultAsync(u =>
                u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (account.OldPassword != null)
            {
                if (hasher.Hash(account.OldPassword) != user.PasswordHash)
                    BadRequest("Old Password mismatch");
                user.PasswordHash = hasher.Hash(account.Password);
            }
            user.Email = account.Email;
            user.MobileNumber = account.Mobile;
            user.IsPrivate = account.IsPrivate;
            db.Update(user);
            await db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async ValueTask<IActionResult> Avatar(string id)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == id);
            if (user.Avatar is null)
            {
                return NoContent();
            }
            return File(user.Avatar, "image/*");
        }

        [HttpPost]
        public async ValueTask<IActionResult> SetAvatar(string id, IFormFile file)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == id);
            var memory = new MemoryStream();
            await file.CopyToAsync(memory);
            user.Avatar = memory.ToArray();
            db.Update(user);
            await db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost, Auth]
        public async ValueTask<IActionResult> Follow(string id)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == id);
            if (user == null)
                return BadRequest("User not found");
            var currentUser = await db.Users.FirstOrDefaultAsync(u =>
                u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));
            var follow = currentUser?.FollowedFollows?.FirstOrDefault(f => f.Followed.Id == user.Id);
            if (follow == null)
            {
                follow = new Follow
                {
                    Followed = user,
                    Follower = currentUser,
                    Accepted = !user.IsPrivate
                };
                await db.AddAsync(follow);
                var notify = new Notification { }; //todo
                await db.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpPost, Auth]
        public async ValueTask<IActionResult> UnFollow(string id)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == id);
            if (user == null)
                return BadRequest("User not found");
            var currentUser = await db.Users.FirstOrDefaultAsync(u =>
                u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == currentUser)
                return Forbid();
            var follow = currentUser?.FollowedFollows?.FirstOrDefault(f => f.Followed.Id == user.Id);
            if (follow != null)
            {
                db.Remove(follow);
                await db.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpPost]
        public async ValueTask<IActionResult> List(
            [FromQuery]string username,
            [FromQuery]bool follower,
            [FromQuery]bool following,
            [FromBody]ReqVm req
            )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var currentUser = await db.Users.FirstOrDefaultAsync(u =>
                u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));

            IEnumerable<User> preUsers;

            if (username is null)
            {
                preUsers = db.Users;
            }
            else
            {
                if ((follower && following) || (!follower && !following))
                    return BadRequest();
                var preUser = await db.Users.FirstOrDefaultAsync(u => u.UserName == username);
                preUsers = follower ?
                    preUser.Followers :
                    preUser.Followeds;
            }



            //preUsers = req.TowardOlds ? preUsers.;

            if (req.LastGuttedId != null)
            {
                preUsers = preUsers.SkipWhile(u => u.Id != req.LastGuttedId).Skip(1);
            }
            preUsers = preUsers.Take(req.MaxCountToGet);

            var users = preUsers.ToList().Select(user => new UserVm
            {
                Id = user.Id,
                UserName = user.UserName,
                IsFollowed = currentUser?.FollowedFollows?.Select(ff => ff.Followed)?
                    .Any(u => u.Id == user.Id) ?? false,
                IsFollowRequested = username is null ? false : user.FollowerFollows
                    .FirstOrDefault(f => f.Follower.UserName == username)?
                    .Accepted ?? false

            }).Where(u => u.UserName != username);
            return Ok(users);
        }

        [HttpPost]
        public async ValueTask<IActionResult> Search([FromQuery]string s)
        {
            var users = await Task.Run(() =>
            {
                var users = db.Users.Where(u => u.UserName.StartsWith(s) || u.UserName.Contains(s))
                    .Take(10);
                return users;
            });
            return Ok(users.Select(u => new { u.UserName }));
        }
    }
}
