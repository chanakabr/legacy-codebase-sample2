using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManager
{
    public abstract class KalturaEvent
    {
        public TVinciShared.CoreObject Object;

        public KalturaEvent(TVinciShared.CoreObject coreObject)
        {
            this.Object = coreObject;
        }
    }
}
