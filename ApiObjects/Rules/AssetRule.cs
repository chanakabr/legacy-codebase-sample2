using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Rules
{
    public class AssetRule
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<AssetRuleCondition> Conditions { get; set; }
        public List<AssetRuleAction> Actions { get; set; }
    }

    public class AssetRuleAction
    {
    }

    public class AssetRuleCondition
    {      
    }
}
