using System.Threading.Tasks;

namespace EventBus.Abstraction
{
    public interface IServiceEventHandler<in TEvent> where TEvent: ServiceEvent
    {
        Task Handle(TEvent serviceEvent);
    }
}