using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManager
{
    public class EventManager
    {
        private static List<BaseEventConsumer> eventConsumers;

        static EventManager()
        {
            eventConsumers = new List<BaseEventConsumer>();
            eventConsumers.Add(new NotificationEventConsumer());
        }

        public static void Init()
        {
        }

        public static void Subscribe(BaseEventConsumer consumer)
        {
            eventConsumers.Add(consumer);
        }

        public static void HandleEvent(KalturaEvent kalturaEvent)
        {
            foreach (var consumer in eventConsumers)
            {
                consumer.HandleEvent(kalturaEvent);
            }
        }
    }
}
