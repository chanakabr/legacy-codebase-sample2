using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MCGroupRules
{
    public class MCTrialSubscriptionAfterEnd: MCImplementationBase
    {

        public MCTrialSubscriptionAfterEnd(int groupid, int ruleid, int ruletype, int rulefreqid, int subscriptionid)
            : base(groupid, ruleid, ruletype, rulefreqid, subscriptionid)
        {

        }

        //Gets all the users that apply to the rule
        public override List<int> GetRuleUsersGuids(int groupID, int ruleType, int ruleInnerFrequency, int subcriptionid)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            DataTable userIdDt = new DataTable();

            selectQuery += string.Format("select SITE_USER_GUID, SUBSCRIPTION_CODE, MAX(START_DATE) as START_DATE  from subscriptions_purchases WHERE GROUP_ID={0} AND SUBSCRIPTION_CODE={1} AND IS_RECURRING_STATUS=0 AND IS_ACTIVE=1 GROUP BY SITE_USER_GUID , SUBSCRIPTION_CODE, START_DATE", groupID, SubscriptionID);
            selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
            selectQuery.Execute("query", true);
            userIdDt = selectQuery.Table("query");
            selectQuery.Finish();

            DataView view = new DataView(userIdDt);
            DataTable distinctSubscriptionCodes = view.ToTable(true, "SUBSCRIPTION_CODE");

            selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "SELECT s.ID, um.FULL_LIFE_CYCLE_MIN FROM subscriptions s, usage_modules um  WHERE s.USAGE_MODULE_CODE = um.ID AND S.ID IN (";

            int i = 0;
            foreach (DataRow dr in distinctSubscriptionCodes.Rows)
            {
                i++;
                selectQuery += dr["SUBSCRIPTION_CODE"].ToString();
                if (i < distinctSubscriptionCodes.Rows.Count)
                {
                    selectQuery += ",";
                }
            }
            selectQuery += ")";
            selectQuery.SetConnectionKey("pricing_connection");
            selectQuery.Execute("query", true);
            DataTable lifeCycleDt = selectQuery.Table("query");
            selectQuery.Finish();

            List<int> retValUserGuidList = new List<int>();
            foreach (DataRow lifeCycleDr in lifeCycleDt.Rows)
            {
                foreach (DataRow userGuidDr in userIdDt.Rows)
                {
                    DateTime subsCycleEnd = DateTime.Parse(userGuidDr["START_DATE"].ToString()).AddDays(MCUtils.GetDaysFromMinPeriodId(int.Parse(lifeCycleDr["FULL_LIFE_CYCLE_MIN"].ToString())));
                    int cycleDays = MCUtils.GetDaysFromMinPeriodId(ruleInnerFrequency);
                    if (subsCycleEnd.AddDays(cycleDays) == DateTime.Today)
                    {
                        retValUserGuidList.Add(int.Parse(userGuidDr["SITE_USER_GUID"].ToString()));
                    }
                }
            }

            return retValUserGuidList;
        }
    }
}
