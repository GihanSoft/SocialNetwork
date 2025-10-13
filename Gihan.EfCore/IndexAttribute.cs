using System;

namespace Gihan.EfCore;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class IndexAttribute : Attribute
{
    public bool IsUniqueKey { get; set; } = false;
    public IndexAttribute() { }
}
