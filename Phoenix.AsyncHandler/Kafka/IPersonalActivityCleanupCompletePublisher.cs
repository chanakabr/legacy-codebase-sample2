namespace Phoenix.AsyncHandler.Kafka
{
    public interface IPersonalActivityCleanupCompletePublisher
    {
        void Publish(long partnerId, long key);
    }
}