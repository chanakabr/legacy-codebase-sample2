using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class GroupOperator
    {
        public UIData UIData { get; set; }
        
        public int ID { get; set; }
        
        public string Name { get; set; }
        
        public eOperatorType Type { get; set; }

        public string LoginUrl { get; set; }

        public int SubGroupID { get; set; }

        public Scope[] Scopes { get; set; }

        public string GroupUserName { get; set; }

        public string GroupPassword { get; set; }

        public string LogoutURL { get; set; }
    }

    public class UIData
    {
        public string ColorCode { get; set; }
        
        public int picID { get; set; }
    }

    public class Scope
    {
        public string Name { get; set; }

        public string LoginUrl { get; set; }

        public string LogoutUrl { get; set; }
    }

    public enum eOperatorType
    {

        /// <remarks/>
        OAuth,

        /// <remarks/>
        API,

        /// <remarks/>
        TVinci,

        /// <remarks/>
        SAML,
    }
}
