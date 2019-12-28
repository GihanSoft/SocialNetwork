using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SocialNetwork.Models
{
    public class User : Identity.Models.UserBase
    {
        public byte[] Avatar { get; set; }
        public bool IsPrivate { get; set; }

        [NotMapped]
        public IEnumerable<User> Followers => FollowerFollows.Select(f => f.Follower);
        [NotMapped]
        public IEnumerable<User> Followeds => FollowedFollows.Select(f => f.Followed);

        public virtual ICollection<Follow> FollowerFollows
        { get; set; }
        public virtual ICollection<Follow> FollowedFollows { get; set; }

        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }

        public override string ToString()
        {
            return UserName;
        }
    }
}
