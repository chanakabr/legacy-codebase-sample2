using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class GroupOperator
    {
        public UIData uiData { get; set; }
        
        public int id { get; set; }
        
        public string name { get; set; }
        
        public eOperatorType type { get; set; }

        public string loginUrl { get; set; }

        public int subGroupID { get; set; }

        public Scope[] scopes { get; set; }

        public string groupUserName { get; set; }

        public string groupPassword { get; set; }

        public string logoutURL { get; set; }
    }
}
