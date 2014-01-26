using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class GroupRule
    {
        public int ruleID { get; set; }

        public string name { get; set; }

        public int tagTypeID { get; set; }

        public string dynamicDataKey { get; set; }

        public string tagValue { get; set; }

        public string[] allTagValues { get; set; }

        public bool isActive { get; set; }

        public eBlockType blockType { get; set; }

        public eGroupRuleType groupRuleType { get; set; }
    }
}
