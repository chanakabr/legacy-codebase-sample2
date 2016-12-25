using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManager.Events
{
    public class KalturaObjectEvent : KalturaEvent
    {
        public ApiObjects.CoreObject Object
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

        public KalturaObjectEvent(int groupId = 0, ApiObjects.CoreObject coreObject = null, string type = null) : base(groupId)
        {
            this.Object = coreObject;
            this.type = type;
        }
    }
}
