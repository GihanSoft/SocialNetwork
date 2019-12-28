using SocialNetwork.EfCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models
{
    public class Post : EntityBase
    {
        public string Text { get; set; }

        [Index]
        public DateTime Time { get; set; }

        [Required]
        public virtual User Sender { get; set; }
        public virtual Post Parent { get; set; }

        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Post> Comments { get; set; }
    }
}
