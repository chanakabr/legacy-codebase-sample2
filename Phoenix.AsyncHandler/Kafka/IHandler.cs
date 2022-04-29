using OTT.Lib.Kafka;

namespace Phoenix.AsyncHandler.Kafka
{
    /// <summary>
    /// Long-running handler, which consumes kafka messages 
    /// </summary>
    /// <typeparam name="T">type of kafka message</typeparam>
    public interface IHandler<T>
    {
        /// <summary>
        /// Function to process a message
        /// </summary>
        /// <param name="consumeResult"></param>
        /// <returns></returns>
        HandleResult Handle(ConsumeResult<string, T> consumeResult);
    }
}
