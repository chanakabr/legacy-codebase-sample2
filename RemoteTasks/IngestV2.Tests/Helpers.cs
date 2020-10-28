using System;
using System.Collections.Generic;

namespace IngestV2.Tests
{
    public static class Helpers
    {
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<int, T> action)
        {
            // argument null checking omitted
            int i = 0;
            foreach (T item in sequence)
            {
                action(i, item);
                i++;
            }
        }
    }
}