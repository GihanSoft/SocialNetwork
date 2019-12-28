using SocialNetwork.EfCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialNetwork.Models.ViewModels
{
    public class PostVm : EntityBase
    {
        public string Sender { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
        public long LikesCount { get; set; }
        public bool Liked { get; set; }
    }

    public static class PostVmEx
    {
        public static PostVm ConvertToVm(this Post post, User user)
        {
            var postVm = new PostVm
            {
                Id = post.Id,
                Sender = post.Sender.UserName,
                Text = post.Text,
                Time = post.Time,
                Liked = user is null ? false : post.Likes.Any(l => l.Liker.Id == user.Id),
                LikesCount = post.Likes.Count
            };
            return postVm;
        }

        public static IEnumerable<PostVm> ConvertToVm(this IEnumerable<Post> posts, User user)
        {
            return posts.Select(p => p.ConvertToVm(user));
        }
        public static IQueryable<PostVm> ConvertToVm(this IQueryable<Post> posts, User user)
        {
            return posts.Select(p => p.ConvertToVm(user));
        }
    }
}
