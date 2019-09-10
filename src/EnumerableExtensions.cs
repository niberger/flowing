using System.Collections.Generic;
using System.Linq;

namespace Flowing
{
    public static class EnumerableExtensions
    {
        public static IFlow<IEnumerable<T>> ToFlow<T>(this IEnumerable<IFlow<T>> flowCollection)
        {
            IFlow<IEnumerable<T>> InnerConcat(IFlow<IEnumerable<T>> collFlow, IFlow<T> flow)
            {
                return from coll in collFlow
                    from val in flow
                    select coll.Concat(new List<T> {val});
            }

            return flowCollection.Aggregate(Flow.Return(Enumerable.Empty<T>()), InnerConcat);
        }
    }
}