using SocialNetwork.Identity;
using SocialNetwork.Identity.Services;
using SocialNetwork.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialNetwork.Data;
using SocialNetwork.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SocialNetwork.Controllers
{
    public class PostController : ControllerBase
    {
        private readonly ILogger<HomeController> logger;
        private readonly AppDbContext db;

        public PostController(
            ILogger<HomeController> logger,
            AppDbContext db
            )
        {
            this.logger = logger;
            this.db = db;
        }

        [HttpPost]
        public async ValueTask<IActionResult> View(long? id, [FromBody]ReqVm postReq)
        {
            if (!ModelState.IsValid && (postReq != null || id == null))
            {
                return BadRequest(ModelState);
            }

            var user = await db.Users.FirstOrDefaultAsync(u =>
                u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (id != null)
            {
                var prePost = await db.Posts.FindAsync(id);
                return Ok(prePost.ConvertToVm(user));
            }
            IEnumerable<Post> prePosts;
            var yesterday = DateTime.Now - TimeSpan.FromDays(1);
            if (user != null)
                prePosts = user.Followeds.SelectMany(u => u.Posts);
            else
                prePosts = db.Posts.Where(p => !p.Sender.IsPrivate && p.Time > yesterday);
            prePosts = postReq.TowardOlds ?
                prePosts.OrderByDescending(p => p.Time) :
                prePosts.OrderBy(p => p.Time);
            if (postReq.LastGuttedId != null)
            {
                prePosts = prePosts.SkipWhile(p => p.Id != postReq.LastGuttedId).Skip(1);
            }
            prePosts = prePosts.Take(postReq.MaxCountToGet);
            var posts = prePosts.ConvertToVm(user);
            return Ok(posts);
        }
        [HttpPost]
        public async ValueTask<IActionResult> UserPosts(string id, [FromBody]ReqVm postReq)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == id);
            if (user == null || !ModelState.IsValid)
                return BadRequest("User not found");
            var currentUser = await db.Users.FirstOrDefaultAsync(u =>
                u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user.IsPrivate && currentUser != null)
            {
                var follow = user.FollowerFollows.FirstOrDefault(f => f.Follower == currentUser);
                if (follow is null || !follow.Accepted)
                {
                    return BadRequest("User is Private");
                }
            }
            IEnumerable<Post> prePosts = user.Posts;
            prePosts = postReq.TowardOlds ?
                prePosts.OrderByDescending(p => p.Time) :
                prePosts.OrderBy(p => p.Time);
            if (postReq.LastGuttedId != null)
            {
                prePosts = prePosts.SkipWhile(p => p.Id != postReq.LastGuttedId).Skip(1);
            }
            prePosts = prePosts.Take(postReq.MaxCountToGet);
            var posts = prePosts.ConvertToVm(user);
            return Ok(posts);
        }

        [HttpPost, Auth]
        public async ValueTask<IActionResult> Send([FromBody]PostVm post)
        {
            var user = await db.Users.FirstOrDefaultAsync(u =>
                u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));
            var postTemp = new Post
            {
                Text = post.Text,
                Sender = user,
                Time = DateTime.Now
            };
            await db.AddAsync(postTemp);
            await db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost, Auth]
        public async ValueTask<IActionResult> Like([FromBody]long? postId)
        {
            if (postId is null)
                return BadRequest();
            var user = await db.Users.FirstOrDefaultAsync(u =>
                u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));

            var post = await db.Posts.FindAsync(postId);
            var like = await db.Likes.FirstOrDefaultAsync(l => l.Liker == user && l.Post == post);
            if (like is null)
            {
                like = new Like
                {
                    Liker = user,
                    Post = post
                };
                await db.AddAsync(like);
                var notify = new Notification
                {
                    Message = $"{user.UserName} liked your post",
                    Link = Url.Action(nameof(View), nameof(PostController), new { id = post.Id }),
                    Time = DateTime.Now,
                    User = post.Sender,
                };
                await db.AddAsync(notify);
                await db.SaveChangesAsync();
            }
            return Ok(post.Likes.Count);
        }

        [HttpPost, Auth]
        public async ValueTask<IActionResult> RemoveLike([FromBody]long? postId)
        {
            if (postId is null)
                return BadRequest();
            var user = await db.Users.FirstOrDefaultAsync(u =>
                u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));

            var post = await db.Posts.FindAsync(postId);
            var like = await db.Likes.FirstOrDefaultAsync(l => l.Liker == user && l.Post == post);
            if (like != null)
            {
                db.Remove(like);
                await db.SaveChangesAsync();
            }
            return Ok(post.Likes.Count);
        }


        [HttpPost, Auth]
        public async ValueTask<IActionResult> Delete([FromBody]long? postId)
        {
            if (postId is null)
                return BadRequest();
            var post = await db.Posts.FindAsync(postId);
            if (post == null)
                return Ok();
            var user = await db.Users.FirstOrDefaultAsync(u =>
                u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (post.Sender != user && !User.IsInRole("admin"))
            {
                return Forbid();
            }
            db.Remove(post);
            await db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async ValueTask<IActionResult> Search([FromQuery]string s, [FromBody]ReqVm postReq)
        {
            if (!ModelState.IsValid || postReq is null)
                return BadRequest(ModelState);

            var user = User.Identity.IsAuthenticated ? await db.Users.FirstOrDefaultAsync(u =>
                u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier)) : null;
            var s1p = new SqlParameter("s1", $"{s}%");
            var s2p = new SqlParameter("s2", $"%{s}%");

            var order = postReq.TowardOlds ? "DESC" : "ASC";

            var sql =
$@"DECLARE @tbl TABLE(
    [R]        INT,
    [Id]       BIGINT,
    [Text]     NVARCHAR(MAX),
    [Time]     DATETIMEOFFSET(7),
    [SenderId] INT,
    [ParentId] BIGINT            
    )

INSERT INTO @tbl
SELECT DISTINCT ROW_NUMBER() OVER(ORDER BY [Time] {order}) AS R, * FROM [Posts]
WHERE [Text] LIKE @s1 OR [Text] LIKE @s2;

SELECT * FROM @tbl
ORDER BY [Time] {order}";

            if (postReq.LastGuttedId != null)
            {
                var skipPart =
$@"
OFFSET (
    SELECT TOP 1 R FROM @tbl AS T
    WHERE T.Id = {postReq.LastGuttedId} 
) rows
FETCH NEXT {postReq.MaxCountToGet} ROWS ONLY;";
                sql += skipPart;
            }
            else
                sql = sql.Replace("DISTINCT", $"DISTINCT TOP {postReq.MaxCountToGet}");
            var prePosts = db.Posts.FromSqlRaw(sql, s1p, s2p);
            var posts = prePosts.ToArray().ConvertToVm(user);
            return Ok(posts);
        }
    }
}
