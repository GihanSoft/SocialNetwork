using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Gihan.Identity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SocialNetwork.Data;
using SocialNetwork.Models;
using SocialNetwork.Models.ViewModels;

namespace SocialNetwork.Controllers;

public class PostController(AppDbContext _db) : ControllerBase
{
    private readonly AppDbContext _db = _db;

    [HttpPost, Auth]
    public async ValueTask<IActionResult> View(long? id, [FromBody]ReqVm postReq)
    {
        if (!ModelState.IsValid)
        {
            if (postReq == null && id != null)
            {
                ModelState.Remove("");
            }
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUser = await _db.Users.FirstOrDefaultAsync(u =>
            u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (id != null)
        {
            var prePost = await _db.Posts.FindAsync(id);
            if (prePost.Sender.IsPrivate)
            {
                var hasAccess = (currentUser?.FollowedFollows?.Any(f => f.Followed == prePost.Sender && f.Accepted) ?? false) ||
                    User.IsInRole("Admin");
                if (!hasAccess)
                {
                    return Forbid();
                }
            }

            return Ok(prePost.ConvertToVm(currentUser));
        }

        IEnumerable<Post> prePosts;
        prePosts = currentUser.FollowedFollows.Where(f => f.Accepted)
            .Select(f => f.Followed).SelectMany(u => u.Posts).Where(p => p.Parent == null);
        prePosts = postReq.TowardOlds ?
            prePosts.OrderByDescending(p => p.Time) :
            prePosts.OrderBy(p => p.Time);
        if (postReq.LastGuttedId != null)
        {
            prePosts = prePosts.SkipWhile(p => p.Id != postReq.LastGuttedId).Skip(1);
        }

        prePosts = prePosts.Take(postReq.MaxCountToGet);
        var posts = prePosts.ConvertToVm(currentUser);
        return Ok(posts);
    }

    [HttpPost]
    public async ValueTask<IActionResult> UserPosts(string id, [FromBody]ReqVm postReq)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == id);
        if (user == null || !ModelState.IsValid)
        {
            return BadRequest("User not found");
        }

        var currentUser = await _db.Users.FirstOrDefaultAsync(u =>
            u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (user.IsPrivate)
        {
            var hasAccess = (currentUser?.FollowedFollows?.Any(f => f.Followed == user && f.Accepted) ?? false) ||
                User.IsInRole("Admin");
            if (!hasAccess)
            {
                return Forbid();
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
        var posts = prePosts.ConvertToVm(currentUser);
        return Ok(posts);
    }

    [HttpPost]
    public async ValueTask<IActionResult> Trends([FromBody]ReqVm postReq)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUser = await _db.Users.FirstOrDefaultAsync(u =>
            u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));
        IEnumerable<Post> prePosts;
        prePosts = _db.Posts.Where(p =>
            p.Parent == null &&
            (!p.Sender.IsPrivate || 
                p.Sender.FollowerFollows.Any(f => f.Follower == currentUser && f.Accepted)
            )
        )
            .OrderByDescending(p => p.Likes.Count);
        prePosts = postReq.TowardOlds ?
            prePosts.OrderByDescending(p => p.Time) :
            prePosts.OrderBy(p => p.Time);
        if (postReq.LastGuttedId != null)
        {
            prePosts = prePosts.SkipWhile(p => p.Id != postReq.LastGuttedId).Skip(1);
        }

        prePosts = prePosts.Take(postReq.MaxCountToGet);
        var posts = prePosts.ConvertToVm(currentUser);
        return Ok(posts);
    }

    [HttpPost]
    public async ValueTask<IActionResult> PostComments(long id, [FromBody]ReqVm postReq)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUser = await _db.Users.FirstOrDefaultAsync(u =>
            u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (id == default)
        {
            return BadRequest();
        }

        var post = await _db.Posts.FindAsync(id);
        if (post == default)
        {
            return BadRequest();
        }

        var hasAccess = (currentUser?.FollowedFollows?.Any(f => f.Followed == post.Sender && f.Accepted) ?? false) ||
            User.IsInRole("Admin");
        if (!hasAccess)
        {
            return Forbid();
        }

        IEnumerable<Post> prePosts = post.Comments;
        prePosts = postReq.TowardOlds ?
            prePosts.OrderByDescending(p => p.Time) :
            prePosts.OrderBy(p => p.Time);
        if (postReq.LastGuttedId != null)
        {
            prePosts = prePosts.SkipWhile(p => p.Id != postReq.LastGuttedId).Skip(1);
        }

        prePosts = prePosts.Take(postReq.MaxCountToGet);
        var posts = prePosts.ConvertToVm(currentUser);
        return Ok(posts);
    }
    [HttpPost]
    public async ValueTask<IActionResult> SendComment(long id, [FromBody]PostVm post)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id == default)
        {
            return BadRequest();
        }

        var superPost = await _db.Posts.FindAsync(id);
        if (superPost == default)
        {
            return BadRequest();
        }

        var currentUser = await _db.Users.FirstOrDefaultAsync(u =>
            u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));
        var hasAccess = (currentUser?.FollowedFollows?.Any(f => f.Followed == superPost.Sender && f.Accepted) ?? false) ||
            User.IsInRole("Admin");
        if (!hasAccess)
        {
            return Forbid();
        }

        var postTemp = new Post
        {
            Text = post.Text,
            Sender = currentUser,
            Time = DateTime.Now,
            Parent = superPost
        };
        await _db.AddAsync(postTemp);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost, Auth]
    public async ValueTask<IActionResult> Send([FromBody]PostVm post)
    {
        var currentUser = await _db.Users.FirstOrDefaultAsync(u =>
            u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));
        var postTemp = new Post
        {
            Text = post.Text,
            Sender = currentUser,
            Time = DateTime.Now
        };
        await _db.AddAsync(postTemp);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost, Auth]
    public async ValueTask<IActionResult> Like([FromBody]long? postId)
    {
        if (postId is null)
        {
            return BadRequest();
        }

        var currentUser = await _db.Users.FirstOrDefaultAsync(u =>
            u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));

        var post = await _db.Posts.FindAsync(postId);
        if (post is null)
        {
            return BadRequest("Post not found");
        }

        var like = await _db.Likes.FirstOrDefaultAsync(l => l.Liker == currentUser && l.Post == post);
        if (like is null)
        {
            if (post.Sender.IsPrivate)
            {
                var hasAccess = (currentUser?.FollowedFollows?.Any(f => f.Followed == post.Sender && f.Accepted) ?? false) ||
                        User.IsInRole("Admin");
                if (!hasAccess)
                {
                    return Forbid();
                }
            }

            like = new Like
            {
                Liker = currentUser,
                Post = post
            };
            await _db.AddAsync(like);
            //var notify = new Notification
            //{
            //    Message = $"{currentUser.UserName} liked your post",
            //    Link = Url.Action(nameof(View), nameof(PostController), new { id = post.Id }),
            //    Time = DateTime.Now,
            //    User = post.Sender,
            //};
            //await db.AddAsync(notify);
            await _db.SaveChangesAsync();
        }

        return Ok(post.Likes.Count);
    }

    [HttpPost, Auth]
    public async ValueTask<IActionResult> RemoveLike([FromBody]long? postId)
    {
        if (postId is null)
        {
            return BadRequest();
        }

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));

        var post = await _db.Posts.FindAsync(postId);
        var like = await _db.Likes.FirstOrDefaultAsync(l => l.Liker == user && l.Post == post);
        if (like != null)
        {
            _db.Remove(like);
            await _db.SaveChangesAsync();
        }

        return Ok(post.Likes.Count);
    }

    [HttpPost, Auth]
    public async ValueTask<IActionResult> Delete([FromBody]long? postId)
    {
        if (postId is null)
        {
            return BadRequest();
        }

        var post = await _db.Posts.FindAsync(postId);
        if (post == null)
        {
            return Ok();
        }

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (post.Sender != user && !User.IsInRole("admin"))
        {
            return Forbid();
        }

        _db.Remove(post);
        await _db.SaveChangesAsync();
        return Ok();
    }

//        [HttpPost]
//        public async ValueTask<IActionResult> Search([FromQuery]string s, [FromBody]ReqVm postReq)
//        {
//            if (!ModelState.IsValid || postReq is null)
//                return BadRequest(ModelState);

//            var user = User.Identity.IsAuthenticated ? await db.Users.FirstOrDefaultAsync(u =>
//                u.UserName == User.FindFirstValue(ClaimTypes.NameIdentifier)) : null;
//            var s1p = new SqlParameter("s1", $"{s}%");
//            var s2p = new SqlParameter("s2", $"%{s}%");

//            var order = postReq.TowardOlds ? "DESC" : "ASC";

//            var sql =
//$@"DECLARE @tbl TABLE(
//    [R]        INT,
//    [Id]       BIGINT,
//    [Text]     NVARCHAR(MAX),
//    [Time]     DATETIMEOFFSET(7),
//    [SenderId] INT,
//    [ParentId] BIGINT            
//    )

//INSERT INTO @tbl
//SELECT DISTINCT ROW_NUMBER() OVER(ORDER BY [Time] {order}) AS R, * FROM [Posts]
//WHERE [Text] LIKE @s1 OR [Text] LIKE @s2;

//SELECT * FROM @tbl
//ORDER BY [Time] {order}";

//            if (postReq.LastGuttedId != null)
//            {
//                var skipPart =
//$@"
//OFFSET (
//    SELECT TOP 1 R FROM @tbl AS T
//    WHERE T.Id = {postReq.LastGuttedId} 
//) rows
//FETCH NEXT {postReq.MaxCountToGet} ROWS ONLY;";
//                sql += skipPart;
//            }
//            else
//                sql = sql.Replace("DISTINCT", $"DISTINCT TOP {postReq.MaxCountToGet}");
//            var prePosts = db.Posts.FromSqlRaw(sql, s1p, s2p);
//            var posts = prePosts.ToArray().ConvertToVm(user);
//            return Ok(posts);
//        }
}
