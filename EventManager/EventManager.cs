using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;

namespace EventManager
{
    public class EventManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static List<BaseEventConsumer> eventConsumers;

        static EventManager()
        {
            eventConsumers = new List<BaseEventConsumer>();
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
                try
                {
                    consumer.HandleEvent(kalturaEvent);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Event manager - error when calling consumer {0}, ex = {1}", consumer.GetType().Name, ex);
                }
            }
        }
    }
}
