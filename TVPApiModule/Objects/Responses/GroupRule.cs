using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class GroupRule
    {
        public int rule_id { get; set; }

        public string name { get; set; }

        public int tag_type_id { get; set; }

        public string dynamic_data_key { get; set; }

        public string tag_value { get; set; }

        public string[] all_tag_values { get; set; }

        public bool is_active { get; set; }

        public eBlockType block_type { get; set; }

        public eGroupRuleType group_rule_type { get; set; }
    }
}
