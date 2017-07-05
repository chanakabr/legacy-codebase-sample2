using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Pricing
{
    public class DependencySet : SubscriptionSet
    {
        public long BaseSubscriptionId {get; set;}
	    public List<long> AddOnIds { get; set; }

        public DependencySet()
            : base(SubscriptionSetType.Dependency)
        {
            BaseSubscriptionId = 0;
            AddOnIds = new List<long>();
        }

        public DependencySet(long id, string name, long baseSubscriptionId)
            : base(id, name, ApiObjects.Pricing.SubscriptionSetType.Dependency)
        {
            BaseSubscriptionId = baseSubscriptionId;
            AddOnIds = new List<long>();
        }

        public DependencySet(long id, string name, long baseSubscriptionId, List<long> addOnIds)
            : base(id, name, ApiObjects.Pricing.SubscriptionSetType.Dependency)
        {
            BaseSubscriptionId = baseSubscriptionId;
            AddOnIds = new List<long>(addOnIds);
        }
    }
}
