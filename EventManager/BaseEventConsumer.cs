using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManager
{
    public abstract class BaseEventConsumer
    {
        public abstract bool ShouldConsume(KalturaEvent kalturaEvent);

        public eEventConsumptionResult HandleEvent(KalturaEvent kalturaEvent)
        {
            eEventConsumptionResult result = eEventConsumptionResult.None;

            if (ShouldConsume(kalturaEvent))
            {
                result = Consume(kalturaEvent);
            }

            return result;
        }

        protected abstract eEventConsumptionResult Consume(KalturaEvent kalturaEvent);
    }

    public enum eEventConsumptionResult
    {
        Success,
        None,
        Failure
    }
}
