using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManager.Events
{
    public class KalturaObjectChangedEvent : KalturaObjectActionEvent
    {
        public ApiObjects.CoreObject PreviousObject
        {
            get;
            set;
        }

        public List<string> ChangedFields
        {
            get;
            set;
        }

        public KalturaObjectChangedEvent(int groupId = 0, ApiObjects.CoreObject newObject = null, ApiObjects.CoreObject previousObject = null, 
            List<string> changedFields = null, string type = null)
            : base (groupId, newObject, eKalturaEventActions.Changed, type)
        {
            this.PreviousObject = previousObject;
            this.ChangedFields = changedFields;
        }
    }
}
