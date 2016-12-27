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

        #endregion

        public KalturaObjectActionEvent(int groupId = 0, ApiObjects.CoreObject coreObject = null, 
            eKalturaEventActions action = eKalturaEventActions.None, string type = null)
            : base (groupId, coreObject, type)
        {
            this.action = action;
        }
    }
}
