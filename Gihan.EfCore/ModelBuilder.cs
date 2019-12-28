using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SocialNetwork.EfCore
{
    public static class GsnModelBuilder
    {
        public static void UseGsnDataAnnotations(this ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    var index = property.PropertyInfo?.GetCustomAttribute<IndexAttribute>();
                    if (index != null)
                    {
                        if (property.ClrType == typeof(string))
                            builder.Entity(entityType.ClrType).Property(property.Name).HasMaxLength(450);
                        var indexBuilder = builder.Entity(entityType.ClrType).HasIndex(property.Name);
                        if (index.IsUniqueKey)
                            indexBuilder.IsUnique();
                    }
                }
            }
        }
    }
}
