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
        private readonly ILogger<UserController> logger;
        private readonly AppDbContext db;

        public UserController(
            ILogger<UserController> logger,
            AppDbContext db
            )
        {
            this.logger = logger;
            this.db = db;
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
                IsFollowed = currentUser?.FollowedFollows?.Any(f => f.Followed == user) ?? false,
                IsFollowAccepted = currentUser?.FollowedFollows?
                    .Any(f => f.Followed == user && f.Accepted) ?? false,
                FollowersCount = user.FollowerFollows.Count - 1,
                FollowingsCount = user.FollowedFollows.Count - 1,
                Email = haveAccessToPrivateData ? user.Email : null,
                Mobile = haveAccessToPrivateData ? user.MobileNumber : null,
                IsPrivate = user.IsPrivate
            };
            return Ok(userVm);
        }

        [HttpGet]
        public async ValueTask<IActionResult> Avatar(string id)
        {
            if (id.EndsWith("JPG", System.StringComparison.OrdinalIgnoreCase) ||
               id.EndsWith("PNG", System.StringComparison.OrdinalIgnoreCase))
                id = id[0..^4];
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == id);
            if (user.Avatar is null)
            {
                return NoContent();
            }
            return File(user.Avatar, "image/jpg");
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

        [HttpPost, Auth]
        public async ValueTask<IActionResult> AcceptFollow(string id)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == id);
            if (user == null)
                return BadRequest("User not found");
            var currentUser = await db.Users.FirstOrDefaultAsync(u =>
                u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));
            var follow = await db.Follows.FirstOrDefaultAsync(f =>
                f.Follower == user && f.Followed == currentUser);
            if (follow is null)
                return BadRequest();
            follow.Accepted = true;
            db.Update(follow);
            await db.SaveChangesAsync();
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
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == username);

            IEnumerable<User> preUsers;

            if (username is null)
            {
                preUsers = db.Users;
            }
            else
            {
                if ((follower && following) || (!follower && !following))
                    return BadRequest();
                var hasAccess = User.IsInRole("Admin") ||
                    user.FollowerFollows.Any(f => f.Follower == currentUser && f.Accepted);
                if (!hasAccess)
                    return Forbid();
                var preUser = await db.Users.FirstOrDefaultAsync(u => u.UserName == username);
                preUsers = follower ?
                    preUser.FollowerFollows.Select(f => f.Follower) :
                    preUser.FollowedFollows.Select(f => f.Followed);
            }

            if (username != null)
            {
                preUsers = req.TowardOlds ?
                preUsers.OrderByDescending(u =>
                    follower ? u.FollowedFollows.FirstOrDefault(f => f.Followed == user).Time :
                    u.FollowerFollows.FirstOrDefault(f => f.Follower == user).Time)
                    :
                preUsers.OrderBy(u =>
                    follower ? u.FollowedFollows.FirstOrDefault(f => f.Followed == user).Time :
                    u.FollowerFollows.FirstOrDefault(f => f.Follower == user).Time);
            }
            else
            {
                preUsers = req.TowardOlds ?
                preUsers.OrderByDescending(u =>
                    u.FollowedFollows.FirstOrDefault(f => f.Followed == u).Time)
                    :
                preUsers.OrderBy(u =>
                    u.FollowedFollows.FirstOrDefault(f => f.Followed == u).Time);
            }

            if (req.LastGuttedId != null)
            {
                preUsers = preUsers.SkipWhile(u => u.Id != req.LastGuttedId).Skip(1);
            }
            preUsers = preUsers.Take(req.MaxCountToGet);

            var users = preUsers.ToList().Select(user => new UserVm
            {
                Id = user.Id,
                UserName = user.UserName,
                IsFollowed = currentUser?.FollowedFollows?
                    .Any(f => f.Followed == user) ?? false,
                IsFollowAccepted = currentUser?.FollowedFollows?
                    .Any(f => f.Followed == user && f.Accepted) ?? false,
                IsFollowRequested = currentUser?.FollowerFollows?
                    .Any(f => f.Follower == user && !f.Accepted) ?? false
            }).Where(u => u.UserName != username);
            return Ok(users);
        }

        [HttpPost]
        public async ValueTask<IActionResult> Search([FromQuery]string s)
        {
            var users = await Task.Run(() =>
            {
                var users = db.Users.Where(u => u.UserName.StartsWith(s) || u.UserName.Contains(s))
                    .Take(5);
                return users;
            });
            return Ok(users.Select(u => new { u.UserName }));
        }
    }
}
