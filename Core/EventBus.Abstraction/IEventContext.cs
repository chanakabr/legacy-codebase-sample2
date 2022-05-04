namespace EventBus.Abstraction
{
    public interface IEventContext
    {
        string RequestId { get; }
        
        long? GroupId { get; }
        
        long? UserId { get; }
    }
}
