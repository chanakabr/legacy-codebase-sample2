using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVinciShared
{
    public static class AsyncHelper
    {
        // Wrapping the async method in task run to avoid deadlock of the syncronization context in some cases.
        // https://stackoverflow.com/questions/17248680/await-works-but-calling-task-result-hangs-deadlocks
        public static T ExecuteAndWait<T>(this Task<T> taskToRun)
        {
            var result = Task.Run(()=> taskToRun).ConfigureAwait(false).GetAwaiter().GetResult();
            return result;
        }

        public static void ExecuteAndWait(this Task taskToRun)
        {
            Task.Run(()=> taskToRun).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static void AddRange<T>(this System.Collections.Concurrent.ConcurrentBag<T> @this, IEnumerable<T> toAdd)
        {
            if (toAdd?.Count() > 0)
            {
                if (@this == null)
                {
                    @this = new System.Collections.Concurrent.ConcurrentBag<T>();
                }

                foreach (var element in toAdd)
                {
                    @this.Add(element);
                }
            }
        }

    }
}
