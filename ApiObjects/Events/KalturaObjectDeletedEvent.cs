using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class KalturaObjectDeletedEvent : KalturaObjectActionEvent
    {
        public long Id
        {
            get;
            set;
        }

        public KalturaObjectDeletedEvent(int groupId = 0, long id = 0, string type = null) : 
            base(groupId, null, eKalturaEventActions.Deleted, type)
        {
            this.Id = id;
        }
    }
}
