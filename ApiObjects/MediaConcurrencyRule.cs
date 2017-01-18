using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace ApiObjects
{
    public class MediaConcurrencyRule
    {
        public int RuleID { get; set; }
        public string Name { get; set; }
        public int TagTypeID { get; set; }        
        public string TagType { get; set; }
        public List<string> AllTagValues { get; set; }
        public int bmId {get; set;}
        public bool IsActive { get; set; }       

        public MediaConcurrencyRule()
        {
            RuleID = 0;
            TagTypeID = 0;
            TagType = string.Empty;            
            Name = string.Empty;
            AllTagValues = new List<string>();
            bmId = 0;
            IsActive = false;
        }

        public MediaConcurrencyRule(int ruleID, int tagTypeID, string tagVal, string name, int isActive, int nBmID)
        {
            RuleID = ruleID;
            TagTypeID = tagTypeID;
            TagType = tagVal;            
            Name = name;

            AllTagValues = new List<string>();
            bmId = nBmID;
            if (isActive == 1)
            {
                IsActive = true;
            }     
        }
    }
}
