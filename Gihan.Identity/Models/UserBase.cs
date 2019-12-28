using SocialNetwork.EfCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetwork.Identity.Models
{
    [Table("Accounts")]
    public class UserBase : EntityBase<int>
    {
        [Index(IsUniqueKey = true)]
        [Required]
        public string UserName { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Index(IsUniqueKey = true)]
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }

        [Index(IsUniqueKey = true)]
        public string MobileNumber { get; set; }
        public bool MobileNumberConfirmed { get; set; }

        [Index]
        public bool IsActive { get; set; }

        public virtual ICollection<AccessToken> AccessTokens { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
