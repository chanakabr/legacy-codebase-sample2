using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;

namespace EventManager.Events
{
    public class NotifyObjectCreatedEvent : NotificationEvent
    {
        public NotifyObjectCreatedEvent(CoreObject coreObject) : base(coreObject)
        {

        }
    }
}
