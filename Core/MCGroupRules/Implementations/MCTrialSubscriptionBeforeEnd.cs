using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MCGroupRules
{
    public class MCTrialSubscriptionBeforeEnd : MCImplementationBase
    {

        public MCTrialSubscriptionBeforeEnd(int groupid, int ruleid, int ruletype, int rulefreqid, int subscriptionid)
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
                    string id = userGuidDr["SITE_USER_GUID"].ToString();
                    DateTime subsCycleEnd = DateTime.Parse(userGuidDr["START_DATE"].ToString()).AddDays(MCUtils.GetDaysFromMinPeriodId(int.Parse(lifeCycleDr["FULL_LIFE_CYCLE_MIN"].ToString())));
                    int cycleDays = 0 - MCUtils.GetDaysFromMinPeriodId(ruleInnerFrequency);
                    if (subsCycleEnd.AddDays(cycleDays).Date == DateTime.Today)
                    {
                        retValUserGuidList.Add(int.Parse(userGuidDr["SITE_USER_GUID"].ToString()));
                    }
                }
            }

            return retValUserGuidList;
        }

        internal override void SetMcObjMergeVars()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += string.Format("SELECT u.EMAIL_ADD, u.FIRST_NAME as '*|FNAME|*' FROM users u WHERE ID IN ({0})", MCUtils.IntListToCsvString(UserGuidList));
            selectQuery.SetConnectionKey("users_connection");
            selectQuery.Execute("query", true);
            if (selectQuery.Table("query").DefaultView.Count > 0)
            {
                foreach (DataRow row in selectQuery.Table("query").Rows)
                {
                    MCPerRecipientMergeVars UserMergeVars = new MCPerRecipientMergeVars() { rcpt = row["EMAIL_ADD"].ToString() };
                    MCGlobalMergeVars var = new MCGlobalMergeVars();
                    foreach (DataColumn col in selectQuery.Table("query").Columns)
                    {
                        if (col.ColumnName != "EMAIL_ADD" && col.ColumnName != "ID")
                        {
                            var.name = col.ColumnName;
                            var.content = row[col.ColumnName].ToString();
                            UserMergeVars.vars.Add(var);
                        }
                    }
                    mcObj.message.merge_vars.Add(UserMergeVars);
                }
            }
            selectQuery.Finish();
        }
    }
}
