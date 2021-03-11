using System;
using System.Reflection;
using KLogMonitor;
using Polly;

namespace EpgNotificationHandler.EpgCache
{
    public static class PolicyHandler
    {
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        private static readonly Func<int, TimeSpan> SleepDurationProvider =
            retryAttempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryAttempt));

        private static Action<Exception, TimeSpan, int, Context> LogWarning(int retryCount) =>
            (ex, time, attempt, ctx) =>
                Logger.Warn($"attempt [{attempt}/{retryCount}] failed, waiting for:[{time.TotalSeconds}] seconds.", ex);

        public static IAsyncPolicy WaitAndRetry(int retryCount = 3) =>
            Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(retryCount, SleepDurationProvider, LogWarning(retryCount));
    }
}