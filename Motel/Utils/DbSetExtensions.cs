using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Motel.Utils;

public static class DbSetExtensions
{
    public static EntityEntry<TEntity> AttachModified<TEntity>(
        this DbSet<TEntity> dbSet,
        TEntity entity,
        IEnumerable<string> changedProperties,
        (string from, string to)[]? maps = null
    ) where TEntity : class
    {
        maps ??= Array.Empty<(string from, string to)>();
        
        var entry = dbSet.Attach(entity);

        foreach (string property in changedProperties)
        {
            string propertyName = property;
            foreach ((string from, string to) in maps)
            {
                if (propertyName != from) continue;
                
                propertyName = to;
                break;
            }

            entry.Property(propertyName).IsModified = true;
        }

        return entry;
    }
}