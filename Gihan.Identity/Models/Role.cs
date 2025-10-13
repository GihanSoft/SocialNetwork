using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace Gihan.Identity.Models;

[Index(nameof(Name))]
public class Role
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; }
    public string Title { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; }
}
