using System.Collections.Generic;

namespace ApplicationAuth.Common.Extensions
{
    public static class CollectionExtensions
    {
        public static ICollection<TResult> Empty<TResult>(this ICollection<TResult> collection)
        {
            return new List<TResult>();
        }

        public static HashSet<TResult> ToHashSet<TResult>(this IEnumerable<TResult> enumerable)
        {
            enumerable.ThrowsWhenNull(nameof(enumerable));

            var hashSet = new HashSet<TResult>();

            // TODO: Remake on 'yeild'
            foreach (var item in enumerable)
            {
                hashSet.Add(item);
            }

            return hashSet;
        }
    }
}
