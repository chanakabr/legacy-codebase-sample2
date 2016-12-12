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

            // If synchronized, do this reuglarly 
            if (kalturaEvent.IsSynchronized)
            {
                if (ShouldConsume(kalturaEvent))
                {
                    result = Consume(kalturaEvent);
                }
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    if (ShouldConsume(kalturaEvent))
                    {
                        Consume(kalturaEvent);
                    }
                });

                // async - result will always be true
                result = true;
            }

            return result;
        }

        protected abstract bool Consume(KalturaEvent kalturaEvent);
    }
}
