using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MCGroupRules
{
    public class MCSubscriptionEnded: MCImplementationBase
    {

        public MCSubscriptionEnded(int groupid, int ruleid, int ruletype, int rulefreqid, int subscriptionid)
            : base(groupid, ruleid, ruletype, rulefreqid, subscriptionid)
        {

        }

        //Gets all the users that apply to the rule
        public override List<int> GetRuleUsersGuids(int groupID, int ruleType, int ruleInnerFrequency, int subscriptionid)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            DataTable userIdDt = new DataTable();

            selectQuery += string.Format("select SITE_USER_GUID  from subscriptions_purchases WHERE GROUP_ID={0} AND IS_RECURRING_STATUS=0 AND IS_ACTIVE=1 AND DATEDIFF(DAY, GETDATE(), END_DATE) = 0", groupID);
            selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
            selectQuery.Execute("query", true);
            userIdDt = selectQuery.Table("query");
            selectQuery.Finish();

            List<int> retValUserGuidList = new List<int>();
            foreach (DataRow dr in userIdDt.Rows)
            {
                retValUserGuidList.Add(int.Parse(dr[0].ToString()));
            }

            return retValUserGuidList;
        }
    }
}
