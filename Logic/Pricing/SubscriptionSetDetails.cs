using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Pricing
{
    public class SubscriptionSetDetails
    {

        public Dictionary<long, int> SetsToPrioritiesMap { get; set; }

        public SubscriptionSetDetails()
        {
            SetsToPrioritiesMap = new Dictionary<long, int>();
        }

        public SubscriptionSetDetails(Dictionary<long, int> setsToPrioritiesMap)
        {
            SetsToPrioritiesMap = new Dictionary<long,int>(setsToPrioritiesMap);
        }

        public SubscriptionSetDetails(long setId, int priority)
        {
            SetsToPrioritiesMap = new Dictionary<long, int>() { { setId, priority } };
        }

    }
}
