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

        public virtual string Type
        {
            get
            {
                string result = string.Empty;

                if (this.Object != null)
                {
                    result = this.Object.GetType().Name;
                }

                return result;
            }
        }

        public virtual string Action
        {
            get
            {
                return string.Empty;
            }
        }

        #endregion

        #region Ctor

        public KalturaEvent(ApiObjects.CoreObject coreObject = null, int groupId = 0)
        {
            this.Object = coreObject;
            this.PartnerId = groupId;
        } 

        #endregion
    }
}
