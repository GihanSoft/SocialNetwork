using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Gihan.Identity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SocialNetwork.Data;
using SocialNetwork.Models;
using SocialNetwork.Models.ViewModels;

namespace SocialNetwork.Controllers;

public class PostController(AppDbContext _db) : ControllerBase
{
    private readonly AppDbContext _db = _db;

    [HttpPost, Auth]
    public async ValueTask<IActionResult> View(long? id, [FromBody] ReqVm postReq)
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
                var hasAccess = User.IsInRole("Admin") ||
                    (currentUser?.FollowedFollows?.Any(f => f.Followed == prePost.Sender && f.Accepted) ?? false);
                if (!hasAccess)
                {
                    return Forbid();
                }
            }

            return Ok(prePost.ConvertToVm(currentUser));
        }

        var prePosts = _db.Follows
            .Where(f => f.Follower == currentUser && f.Accepted)
            .SelectMany(f => f.Followed.Posts)
            .Where(p => p.Parent == null)
            ;
        prePosts = postReq.TowardOlds ?
            prePosts.OrderByDescending(p => p.Time) :
            prePosts.OrderBy(p => p.Time);
        if (postReq.LastGuttedId != null)
        {
            prePosts = postReq.TowardOlds
                ? prePosts.Where(p => p.Id < postReq.LastGuttedId)
                : prePosts.Where(p => p.Id > postReq.LastGuttedId);
        }

        prePosts = prePosts.Take(postReq.MaxCountToGet);
        var posts = await prePosts.ConvertToVm(currentUser).ToListAsync();
        return Ok(posts);
    }

    [HttpPost]
    public async ValueTask<IActionResult> UserPosts(string id, [FromBody] ReqVm postReq)
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

        var prePosts = _db.Posts.Where(a => a.Sender == user);
        prePosts = postReq.TowardOlds
            ? prePosts.OrderByDescending(p => p.Time)
            : prePosts.OrderBy(p => p.Time);
        if (postReq.LastGuttedId != null)
        {
            prePosts = postReq.TowardOlds
                ? prePosts.Where(p => p.Id < postReq.LastGuttedId)
                : prePosts.Where(p => p.Id > postReq.LastGuttedId);
        }

        prePosts = prePosts.Take(postReq.MaxCountToGet);
        var posts = await prePosts.ConvertToVm(currentUser).ToListAsync();
        return Ok(posts);
    }

    [HttpPost]
    public async ValueTask<IActionResult> Trends([FromBody] ReqVm postReq)
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
    public async ValueTask<IActionResult> PostComments(long id, [FromBody] ReqVm postReq)
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
    public async ValueTask<IActionResult> SendComment(long id, [FromBody] PostVm post)
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
    public async ValueTask<IActionResult> Send([FromBody] PostVm post)
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
    public async ValueTask<IActionResult> Like([FromBody] long? postId)
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
    public async ValueTask<IActionResult> RemoveLike([FromBody] long? postId)
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

    [HttpDelete, Auth]
    public async ValueTask<IActionResult> Delete(long id)
    {
        var isAdmin = User.IsInRole("admin");
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id && (isAdmin || p.Sender.UserName == username));
        if (post is null)
        {
            return NotFound();
        }

        _db.Remove(post);
        await _db.SaveChangesAsync();
        return Ok();
    }
}
