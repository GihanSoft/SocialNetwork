using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace SocialNetwork.Models;

[Index(nameof(Time))]
public class Post
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string Text { get; set; }

    public DateTime Time { get; set; }

    [Required]
    public virtual User Sender { get; set; }
    public virtual Post Parent { get; set; }

    public virtual ICollection<Like> Likes { get; set; }
    public virtual ICollection<Post> Comments { get; set; }
}
