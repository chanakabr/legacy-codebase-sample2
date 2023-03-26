using System;

namespace IngestHandler.Common.Infrastructure
{
    public class StaticBulkCompletedRetryPolicyConfiguration : IBulkCompletedRetryPolicyConfiguration
    {
        public int Duration { get; } = (int)Math.Ceiling(4.0 * 60);

        public int Timeout => 10;
    }
}
