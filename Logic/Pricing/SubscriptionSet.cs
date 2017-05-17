using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Pricing
{
    public class SubscriptionSet
    {

        public long Id { get; set; }
        public string Name { get; set; }
        public List<long> SubscriptionIds { get; set; }

        public SubscriptionSet()
        {
            Id = 0;
            SubscriptionIds = new List<long>();
        }

        public SubscriptionSet(long id, string name)
        {
            Id = id;
            Name = name;
            SubscriptionIds = new List<long>();
        }

        public SubscriptionSet(long id, string name, List<long> subscriptionIds)
        {
            Id = id;
            Name = name;
            SubscriptionIds = new List<long>(subscriptionIds);
        }

    }
}
