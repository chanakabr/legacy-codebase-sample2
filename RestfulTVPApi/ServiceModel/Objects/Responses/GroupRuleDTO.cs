using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceModel
{
    public class GroupRuleDTO
    {
        public int RuleID { get; set; }

        public string Name { get; set; }

        public int TagTypeID { get; set; }

        public string DynamicDataKey { get; set; }

        public string TagValue { get; set; }

        public string[] AllTagValues { get; set; }

        public bool IsActive { get; set; }

        public eBlockTypeDTO BlockType { get; set; }

        public eGroupRuleTypeDTO GroupRuleType { get; set; }
    }

    public enum eBlockTypeDTO
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

    public enum eGroupRuleTypeDTO
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