using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
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
    }
}
