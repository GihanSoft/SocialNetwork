using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialNetwork.Models;

public class Notification
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string Message { get; set; }
    public string Link { get; set; }
    public bool Seen { get; set; }
    public DateTime Time { get; set; }
    public virtual User User { get; set; }
}
