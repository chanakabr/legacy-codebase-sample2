using System;

namespace EventBus.RabbitMQ
{
    /// <summary>RetryableErrorException will make an event to be re-queued</summary>
    public class RetryableErrorException : Exception
    {
        public RetryableErrorException(Exception exception) : base("retryable error", exception)
        {
        }
    }
}