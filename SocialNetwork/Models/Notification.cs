using SocialNetwork.EfCore;
using System;

namespace SocialNetwork.Models
{
    public class Notification : EntityBase
    {
        public string Message { get; set; }
        public string Link { get; set; }
        public bool Seen { get; set; }
        public DateTime Time { get; set; }
        public virtual User User { get; set; }
    }
}
