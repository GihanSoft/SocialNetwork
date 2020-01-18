using SocialNetwork.EfCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetwork.Models
{
    public class Follow : EntityBase
    {
        public bool Accepted { get; set; }
        public DateTime Time { get; set; }
        [Required]
        public virtual User Follower { get; set; }
        [Required]
        public virtual User Followed { get; set; }

        public override string ToString()
        {
            return $"{Follower.UserName} follows {Followed.UserName}";
        }
    }
}
