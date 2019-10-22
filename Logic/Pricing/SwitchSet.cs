using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Pricing
{
    public class SwitchSet : SubscriptionSet
    {
        //public List<KeyValuePair> SubscriptionSetIdsToPriority;  

        public List<long> SubscriptionIds { get; set; }
        
        public SwitchSet():base()
        {
            SubscriptionIds = new List<long>();

        }
        public SwitchSet(long id, string name)
            : base(id, name)
        {
            SubscriptionIds = new List<long>();
        }
        public SwitchSet(long id, string name, List<long> subscriptionIds)
            : base(id, name, ApiObjects.Pricing.SubscriptionSetType.Switch)
        {
            SubscriptionIds = new List<long>(subscriptionIds);
        }
    }
}
