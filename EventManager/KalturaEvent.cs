using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManager
{
    public abstract class KalturaEvent
    {
        public ApiObjects.CoreObject Object;
        public int GroupId;
        public bool IsSynchronized;

        public KalturaEvent(ApiObjects.CoreObject coreObject = null, int groupId = 0)
        {
            this.Object = coreObject;
            this.GroupId = groupId;
        }
    }
}
