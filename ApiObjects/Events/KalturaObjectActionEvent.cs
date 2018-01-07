using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public enum eKalturaEventActions
    {
        None,
        Added,
        Changed,
        Copied,
        Created,
        Deleted,
        Erased,
        Saved,
        Updated,
        Replaced
    }

    public enum eKalturaEventTime
    {
        Before,
        After,
        Failed
    }

    public class KalturaObjectActionEvent : KalturaObjectEvent
    {
        #region Props

        private eKalturaEventActions action;

        public virtual eKalturaEventActions Action
        {
            get
            {
                return action;
            }
        }

        public eKalturaEventTime Time
        {
            get;
            set;
        }
        
        #endregion

        public KalturaObjectActionEvent(int groupId = 0, ApiObjects.CoreObject coreObject = null, 
            eKalturaEventActions action = eKalturaEventActions.None, eKalturaEventTime time = eKalturaEventTime.After, string type = null) : 
            base(groupId, coreObject, type)
        {
            this.action = action;
            this.Time = time;
        }

        public override string GetSystemName()
        {
            // e.g before_subscriptionpurchase_created
            return string.Format("{0}_{1}_{2}", this.Time, this.Type, this.Action);
        }
    }
}
