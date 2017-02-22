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

        public static List<eEventConsumptionResult> HandleEvent(KalturaEvent kalturaEvent)
        {
            List<eEventConsumptionResult> results = new List<eEventConsumptionResult>();

            foreach (var consumer in eventConsumers)
            {
                eEventConsumptionResult currentResult = eEventConsumptionResult.None;
                try
                {
                    currentResult = consumer.HandleEvent(kalturaEvent);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Event manager - error when calling consumer {0}, ex = {1}", consumer.GetType().Name, ex);
                    currentResult = eEventConsumptionResult.Failure;
                }

                results.Add(currentResult);
            }

            return results;
        }
    }
}
