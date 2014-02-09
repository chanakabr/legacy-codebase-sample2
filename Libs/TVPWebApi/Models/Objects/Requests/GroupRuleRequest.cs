using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TVPWebApi.Models
{
    public class GroupRuleRequest
    {
        public int rule_id { get; set; }
        public string pin_code { get; set; }
        public int is_active { get; set; }
    }
}