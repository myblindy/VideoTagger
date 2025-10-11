using System.Collections.Generic;

namespace VideoTagger.Helpers;

static class ListHelpers
{
    public static void AddRange<T>(this IList<T> list, IEnumerable<T>? items)
    {
        if (items is not null)
            foreach (var item in items)
                list.Add(item);
    }
}
