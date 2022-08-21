using System;
using System.Collections.Generic;
using System.Linq;

namespace EventBus.Kafka.Helpers
{
    public class DictionaryComparer : IEqualityComparer<IReadOnlyDictionary<string, string>>
    {
        public bool Equals(IReadOnlyDictionary<string, string> source, IReadOnlyDictionary<string, string> target)
        {
            if (ReferenceEquals(source, target))
            {
                return true;
            }

            if (ReferenceEquals(source, null))
            {
                return false;
            }

            if (ReferenceEquals(target, null))
            {
                return false;
            }

            if (source.GetType() != target.GetType())
            {
                return false;
            }

            if (source.Count != target.Count)
            {
                return false;
            }

            foreach (var sourceItem in source)
            {
                var keyExistsAndValuesAreEqual = target.TryGetValue(sourceItem.Key, out var value) &&
                                                 string.Compare(value, sourceItem.Value, StringComparison.InvariantCultureIgnoreCase) == 0;
                if (!keyExistsAndValuesAreEqual)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(IReadOnlyDictionary<string, string> obj)
        {
            unchecked
            {
                return obj.Aggregate(0, (accumulator, item) => accumulator + item.Key.GetHashCode() + item.Value.GetHashCode());
            }
        }
    }
}
