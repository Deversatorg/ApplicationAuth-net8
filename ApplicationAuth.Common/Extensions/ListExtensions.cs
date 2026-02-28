using System.Collections.Generic;

namespace ApplicationAuth.Common.Extensions
{
    public static class ListExtensions
    {
        public static List<TResult> Empty<TResult>(this List<TResult> list)
        {
            return new List<TResult>();
        }
    }
}
