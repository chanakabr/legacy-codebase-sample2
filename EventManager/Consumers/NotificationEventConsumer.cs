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
            if (kalturaEvent is NotificationEvent)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override bool Consume(KalturaEvent kalturaEvent)
        {
            throw new NotImplementedException();
        }
    }
}
