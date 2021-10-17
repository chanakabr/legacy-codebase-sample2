using System;
using System.Collections.Generic;
using System.Text;

namespace TVinciShared
{
    public static class StringBuilderExtensions
    {
#if NETFRAMEWORK
        // copy paste from https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Text/StringBuilder.cs
        public static StringBuilder AppendJoin<T>(this StringBuilder sb, string separator,
            IEnumerable<T> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            using (var en = values.GetEnumerator())
            {
                if (!en.MoveNext())
                {
                    return sb;
                }

                T value = en.Current;
                if (value != null)
                {
                    sb.Append(value);
                }

                while (en.MoveNext())
                {
                    sb.Append(separator);
                    value = en.Current;
                    if (value != null)
                    {
                        sb.Append(value);
                    }
                }
            }

            return sb;
        }
#endif
    }
}