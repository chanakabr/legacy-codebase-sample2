using EventManager;

namespace ApiObjects
{
    public class KalturaObjectEvent : KalturaEvent
    {
        public CoreObject Object { get; set; }

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

        public KalturaObjectEvent(int groupId = 0, CoreObject coreObject = null, string type = null) 
            : base(groupId)
        {
            this.Object = coreObject;
            this.type = type;
        }

        public virtual string GetSystemName()
        {
            // e.g subscriptionended
            return string.Format("{0}", this.Type); 
        }
    }
}
