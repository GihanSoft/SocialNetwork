using SocialNetwork.EfCore;
using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models
{
    public class Like : EntityBase
    {
        [Required]
        public virtual Post Post { get; set; }
        [Required]
        public virtual User Liker { get; set; }
    }
}
