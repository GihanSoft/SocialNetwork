using SocialNetwork.EfCore;
using System;

namespace SocialNetwork.Identity.Models
{
    public class UserRole<TUser> : EntityBase<int>
        where TUser : UserBase
    {
        public int UserId { get; set; }
        public virtual TUser User { get; set; }

        public int RoleId { get; set; }
        public virtual Role Role { get; set; }

        public DateTime JoinTime { get; set; }
    }

    public class UserRole : UserRole<UserBase> { }
}
