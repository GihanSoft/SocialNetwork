using System.Collections.Generic;

using Gihan.Identity.Models;

namespace SocialNetwork.Models;

public class User : UserBase
{
    public byte[] Avatar { get; set; }
    public bool IsPrivate { get; set; }

    public virtual ICollection<Follow> FollowerFollows { get; set; }
    public virtual ICollection<Follow> FollowedFollows { get; set; }

    public virtual ICollection<Like> Likes { get; set; }
    public virtual ICollection<Post> Posts { get; set; }
    public virtual ICollection<Notification> Notifications { get; set; }

    public override string ToString()
    {
        return UserName;
    }
}
