using System;

namespace ApiObjects.Cloudfront
{
    public class WaitConfig
    {
        public static readonly WaitConfig Default = new WaitConfig(TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(10));
        
        public WaitConfig(TimeSpan maxWaitDuration, TimeSpan sleepDuration)
        {
            MaxWaitDuration = maxWaitDuration;
            SleepDuration = sleepDuration;
        }

        public TimeSpan MaxWaitDuration { get; }
        public TimeSpan SleepDuration { get; }
        public int RetriesCount() => (int)(MaxWaitDuration.TotalMilliseconds/SleepDuration.TotalMilliseconds);
    }
}