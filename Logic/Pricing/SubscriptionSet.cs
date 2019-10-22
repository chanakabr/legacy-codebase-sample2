using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Pricing
{
    public abstract class SubscriptionSet
    {

        public long Id { get; set; }
        public string Name { get; set; }
        public SubscriptionSetType Type { get; set; }
                

        public SubscriptionSet()
        {
            Id = 0;
            Name = string.Empty;
            Type = SubscriptionSetType.Switch;
        }

        public SubscriptionSet(SubscriptionSetType type)
        {
            Id = 0;
            Name = string.Empty;
            Type = type;
        }

        public SubscriptionSet(long id, string name)
        {
            Id = id;
            Name = name;
            Type = SubscriptionSetType.Switch;
        }
        public SubscriptionSet(long id, string name, SubscriptionSetType type)
        {
            Id = id;
            Name = name;
            Type = type;
        }
    }
}
