namespace IngestHandler.Common.Infrastructure
{
    public interface IBulkCompletedRetryPolicyConfiguration
    {
        /// <summary>
        /// Duration of process in seconds.
        /// </summary>
        int Duration { get; }

        /// <summary>
        /// Duration of timeout in seconds.
        /// </summary>
        int Timeout { get; }
    }
}
