using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MCGroupRules
{
    public class MCSocialInviteTriggered : MCImplementationBase
    {

        public MCSocialInviteTriggered(int groupid, int ruleid, int ruletype, int rulefreqid, int subscriptionid, int userSiteGuid)
            : base(groupid, ruleid, ruletype, rulefreqid, subscriptionid)
        {
            UserGuidList.Add(userSiteGuid);
        }


        public override List<int> GetRuleUsersGuids(int groupID, int ruleType, int ruleInnerFrequency, int subcriptionid)
        {
            return this.UserGuidList = new List<int>();
        }
    }
}
