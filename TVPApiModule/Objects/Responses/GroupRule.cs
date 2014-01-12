using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class GroupRule
    {
        public int RuleID { get; set; }

        public string Name { get; set; }

        public int TagTypeID { get; set; }

        public string DynamicDataKey { get; set; }

        public string TagValue { get; set; }

        public string[] AllTagValues { get; set; }

        public bool IsActive { get; set; }

        public eBlockType BlockType { get; set; }

        public eGroupRuleType GroupRuleType { get; set; }
    }

    public enum eBlockType
    {

        /// <remarks/>
        Allowed,

        /// <remarks/>
        Validation,

        /// <remarks/>
        AgeBlock,

        /// <remarks/>
        Geo,

        /// <remarks/>
        Device,

        /// <remarks/>
        UserType,
    }

    public enum eGroupRuleType
    {

        /// <remarks/>
        Unknown,

        /// <remarks/>
        Parental,

        /// <remarks/>
        Purchase,

        /// <remarks/>
        Device,

        /// <remarks/>
        EPG,
    }
}
