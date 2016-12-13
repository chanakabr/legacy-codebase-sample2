using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManager
{
    public abstract class KalturaEvent
    {
        #region Properties

        public ApiObjects.CoreObject Object
        {
            get;
            set;
        }

        public int PartnerId
        {
            get;
            set;
        }

        private string type;

        public virtual string Type
        {
            get
            {
                string result = string.Empty;

                if (!string.IsNullOrEmpty(type))
                {
                    result = type;
                } 
                else if (this.Object != null)
                {
                    result = this.Object.GetType().Name;
                }

                return result;
            }
        }

        private string action;

        public virtual string Action
        {
            get
            {
                return action;
            }
        }

        #endregion

        #region Ctor

        public KalturaEvent(ApiObjects.CoreObject coreObject = null, int groupId = 0, string type = null, string action = null)
        {
            this.Object = coreObject;
            this.PartnerId = groupId;
            this.type = type;
            this.action = action;
        } 

        #endregion
    }
}
