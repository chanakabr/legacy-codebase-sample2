using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Web.Script.Serialization;

namespace ApiObjects
{
    public class GroupRule
    {
        public int RuleID { get; set; }
        public string Name { get; set; }
        public int TagTypeID { get; set; }
        public string DynamicDataKey { get; set; }
        public string TagValue { get; set; }
        public List<string> AllTagValues { get; set; }
        public bool IsActive { get; set; }
        public eBlockType BlockType { get; set; }
        public eGroupRuleType GroupRuleType { get; set; }
        public bool BlockAnonymous { get; set; }

        [XmlIgnore] [ScriptIgnore]
        public int OrderNum { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
        public int AgeRestriction { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
        public bool AgeLimit { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
        public string TagType { get; set; }

        public GroupRule()
        {
            RuleID = 0;
            TagTypeID = 0;
            TagValue = string.Empty;
            DynamicDataKey = string.Empty;
            Name = string.Empty;
            AllTagValues = new List<string>();
            BlockAnonymous = false;
        }

        public GroupRule(int ruleID, int tagTypeID, string tagVal, string dynamicKey, string name, object ageRestriction, int isActive, eGroupRuleType groupRuleType)
        {
            RuleID = ruleID;
            TagTypeID = tagTypeID;
            TagValue = tagVal;
            DynamicDataKey = dynamicKey;
            Name = name;
            if (ageRestriction != null && ageRestriction != System.DBNull.Value)
            {
                AgeRestriction = (int)ageRestriction;
            }
            AllTagValues = new List<string>();
            if (isActive == 1)
            {
                IsActive = true;
            }
            GroupRuleType = groupRuleType;          
        }

        public GroupRule(int ruleID, int tagTypeID, string tagVal, string dynamicKey, string name, object ageRestriction, int isActive, eGroupRuleType groupRuleType, bool blockAnonymous)
        {
            Initialize(ruleID, tagTypeID, tagVal, dynamicKey, name, ageRestriction, isActive, groupRuleType, blockAnonymous);
                /*
            RuleID = ruleID;
            TagTypeID = tagTypeID;
            TagValue = tagVal;
            DynamicDataKey = dynamicKey;
            Name = name;
            if (ageRestriction != null && ageRestriction != System.DBNull.Value)
            {
                AgeRestriction = (int)ageRestriction;
            }
            AllTagValues = new List<string>();
            if (isActive == 1)
            {
                IsActive = true;
            }
            GroupRuleType = groupRuleType;
            BlockAnonymous = blockAnonymous;*/
        }
        public GroupRule(int ruleID, int tagTypeID, string tagVal, string dynamicKey, string name, object ageRestriction, int isActive, eGroupRuleType groupRuleType, bool blockAnonymous, string tagName)
        {
            Initialize(ruleID, tagTypeID, tagVal, dynamicKey, name, ageRestriction, isActive, groupRuleType, blockAnonymous);
            this.TagType = tagName;
        }

        private void Initialize(int ruleID, int tagTypeID, string tagVal, string dynamicKey, string name, object ageRestriction, int isActive, eGroupRuleType groupRuleType, bool blockAnonymous)
        {
            RuleID = ruleID;
            TagTypeID = tagTypeID;
            TagValue = tagVal;
            DynamicDataKey = dynamicKey;
            Name = name;
            if (ageRestriction != null && ageRestriction != System.DBNull.Value)
            {
                AgeRestriction = (int)ageRestriction;
            }
            AllTagValues = new List<string>();
            if (isActive == 1)
            {
                IsActive = true;
            }
            GroupRuleType = groupRuleType;
            BlockAnonymous = blockAnonymous;
        }
    }
}
