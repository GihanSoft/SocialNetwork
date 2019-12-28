using SocialNetwork.EfCore;
using System;

namespace SocialNetwork.Identity.Models
{
    public class AccessToken<TUser> : EntityBase<Guid>
        where TUser : UserBase
    {
        public int UserId { get; set; }
        public virtual TUser User { get; set; }

        public DateTime ExpiresTime { get; set; }
        public DateTime LastLoginTime { get; set; }
        public string LastLoginIp { get; set; }
        public string LastLoginUserAgent { get; set; }
    }

    public class AccessToken : AccessToken<UserBase> { }
}
