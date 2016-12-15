using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;

namespace EventManager.Events
{
    public class ObjectUpdatedEvent : KalturaEvent
    {
        public override string Action
        {
            get
            {
                return "update";
            }
        }

        public ObjectUpdatedEvent(CoreObject coreObject)
            : base(coreObject)
        {

        }
    }
}
