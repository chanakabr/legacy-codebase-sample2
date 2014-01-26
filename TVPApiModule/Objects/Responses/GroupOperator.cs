using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class GroupOperator
    {
        public UIData ui_data { get; set; }
        
        public int id { get; set; }
        
        public string name { get; set; }
        
        public eOperatorType type { get; set; }

        public string login_url { get; set; }

        public int sub_group_id { get; set; }

        public Scope[] scopes { get; set; }

        public string group_user_name { get; set; }

        public string group_password { get; set; }

        public string logout_url { get; set; }
    }
}
