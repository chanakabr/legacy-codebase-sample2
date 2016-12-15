using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;

namespace EventManager.Events
{
    public class ObjectCreatedEvent : KalturaEvent
    {
        public override string Action
        {
            get
            {
                return "create";
            }
        }

        public ObjectCreatedEvent(CoreObject coreObject, int partnerId)
            : base(coreObject, partnerId)
        {

        }
    }
}
