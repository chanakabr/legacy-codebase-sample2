using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;

namespace EventManager
{
    public class NotificationEvent : KalturaEvent
    {
        public NotificationEvent(CoreObject coreObject) : base(coreObject)
        {

        }
    }
}
