using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventBus.Abstraction
{
    public interface IServiceEventHandler<in TEvent> where TEvent: ServiceEvent
    {
        Task Handle(TEvent serviceEvent);
    }

    public static class IServiceEventHandlerUtils
    {
        public static bool IsInterfaceAnyGenericOfIServiceHandler(Type i)
        {
            return i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IServiceEventHandler<>);
        }

        public static bool IsTypeImplementsIServiceHandler(Type t)
        {
            var implementedInterfaces = t.GetInterfaces();
            return implementedInterfaces.Any(IsInterfaceAnyGenericOfIServiceHandler);
        }
    }
}