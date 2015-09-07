using System;
using System.Collections.Generic;

namespace Code.ConvertingForToLinq
{
    public static class LinqReplacer
    {
        public static IEnumerable<T> Replace<T>(
            this IEnumerable<T> elements,
            Func<T, bool> match,
            Func<T, IEnumerable<T>> replacer)
        {
            bool hasReplaced = false;
            foreach (var element in elements)
            {
                if (!hasReplaced && match(element))
                {
                    hasReplaced = true;
                    foreach (var replaced in replacer(element))
                    {
                        yield return replaced;
                    }
                }
                else
                {
                    yield return element;
                }
            }
        }
    }
}