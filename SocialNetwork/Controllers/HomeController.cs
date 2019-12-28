using SocialNetwork.Identity;
using SocialNetwork.Identity.Services;
using SocialNetwork.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SocialNetwork.Data;
using SocialNetwork.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SocialNetwork.Controllers
{
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> logger;
        private readonly AppDbContext db;
        private readonly IUserManager<User> userManager;

        public HomeController(
            ILogger<HomeController> logger,
            AppDbContext db,
            IUserManager<User> userManager
            )
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
        }


        [HttpGet("API/")]
        [Auth]
        public async ValueTask<IActionResult> Index()
        {
            var user = await userManager.FindByUserNameAsync(
                User.FindFirstValue(ClaimTypes.NameIdentifier));
            var prePosts = user.FollowedFollows.Select(f => f.Followed).SelectMany(u => u.Posts).Take(10);
            var posts = prePosts.Select(post => new PostVm
            {
                Id = post.Id,
                Sender = post.Sender.UserName,
                Text = post.Text,
                Time = post.Time,
                Liked = post.Likes.Any(l => l.Liker.Id == user.Id),
                LikesCount = post.Likes.Count
            });
            return Ok(posts);
        }
    }
}
