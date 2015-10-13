using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVinciShared
{
    public class ListUtils
    {
        public static IEnumerable<T> Page<T>(IEnumerable<T> original, int pageSize, int pageIndex, out bool illegalRequest)
        {
            IEnumerable<T> output = null;

            illegalRequest = false;

            if (pageSize < 0 || pageIndex < 0)
            {
                // illegal parameters
                illegalRequest = true;
            }
            else
            {
                if (pageSize == 0 && pageIndex == 0)
                {
                    // return all results
                    output = original;
                }
                else
                {
                    // apply paging on results 
                    output = original.Skip(pageSize * pageIndex).Take(pageSize);
                }
            }

            return output;
        }
    }
}
