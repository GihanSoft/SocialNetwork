using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gihan.Identity.Models;

public class AccessToken<TUser>
    where TUser : UserBase
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public int UserId { get; set; }
    public virtual TUser User { get; set; }

    public DateTime ExpiresTime { get; set; }
    public DateTime LastLoginTime { get; set; }
    public string LastLoginIp { get; set; }
    public string LastLoginUserAgent { get; set; }
}

public class AccessToken : AccessToken<UserBase> { }
