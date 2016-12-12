using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;

namespace EventManager.Events
{
    public class ObjectCreatedEvent : NotificationEvent
    {
        public ObjectCreatedEvent(CoreObject coreObject) : base(coreObject)
        {

        }
    }
}
