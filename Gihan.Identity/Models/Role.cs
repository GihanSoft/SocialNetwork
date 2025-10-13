using System;
using System.Collections.Generic;

using Gihan.EfCore;

namespace Gihan.Identity.Models;

public class Role : EntityBase<int>
{
    [Index(IsUniqueKey = true)]
    public string Name { get; set; }

    public string Title { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; }
}
