using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace Gihan.Identity.Models;

[Table("Accounts")]
[Index(nameof(UserName), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(MobileNumber), IsUnique = true)]
[Index(nameof(IsActive))]
public class UserBase
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string UserName { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }

    public string MobileNumber { get; set; }
    public bool MobileNumberConfirmed { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<AccessToken> AccessTokens { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; }
}
