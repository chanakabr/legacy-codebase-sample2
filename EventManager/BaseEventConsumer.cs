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

        public bool HandleEvent(KalturaEvent kalturaEvent)
        {
            bool result = false;

            if (ShouldConsume(kalturaEvent))
            {
                result = Consume(kalturaEvent);
            }

            return result;
        }

        protected abstract bool Consume(KalturaEvent kalturaEvent);
    }
}
