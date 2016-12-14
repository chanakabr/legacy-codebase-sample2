using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManager
{
    public class NotificationEventConsumer : BaseEventConsumer
    {
        public override bool ShouldConsume(KalturaEvent kalturaEvent)
        {
            bool shouldConsume = false;

            if (kalturaEvent is NotificationEvent)
            {
                shouldConsume = true;
            }

            return shouldConsume;
        }

        protected override bool Consume(KalturaEvent kalturaEvent)
        {
            // dummy consumer
            return false;
        }
    }
}
