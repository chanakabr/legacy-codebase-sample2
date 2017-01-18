using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    /// <summary>
    /// Represent data from users_group_rules table at Tvinci db. 
    /// </summary>
    public class UserGroupRuleResponse
    {
        public int SiteGuid { get; set; }
        public int RuleID { get; set; }
        public string ChangePinToken { get; set; }  
        public UserGroupRuleResponseStatus ResponseStatus { get; set; }

        public UserGroupRuleResponse() { }

        public void Initialize(UserGroupRuleResponseStatus responseStatus)
        {
            SiteGuid = 0;
            RuleID = 0;
            ChangePinToken = string.Empty; 
            ResponseStatus = responseStatus;
        }

        public void Initialize(int siteGuid, int ruleID, string changePinToken, UserGroupRuleResponseStatus responseStatus)
        {
            SiteGuid = siteGuid;
            RuleID = ruleID;
            ChangePinToken = changePinToken;
            ResponseStatus = responseStatus;
        }
    }
}
