using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gihan.Identity.Models;

public class UserRole<TUser>
    where TUser : UserBase
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int UserId { get; set; }
    public virtual TUser User { get; set; }

    public int RoleId { get; set; }
    public virtual Role Role { get; set; }

    public DateTime JoinTime { get; set; }
}

public class UserRole : UserRole<UserBase> { }
