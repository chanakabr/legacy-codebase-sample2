using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MCGroupRules
{
    public class MCPaymentFailed : MCImplementationBase
    {

        public MCPaymentFailed(int groupid, int ruleid, int ruletype, int rulefreqid, int subscriptionid)
            : base(groupid, ruleid, ruletype, rulefreqid, subscriptionid)
        {

        }

        //Gets all the users that apply to the rule
        public override List<int> GetRuleUsersGuids(int groupID, int ruleType, int ruleInnerFrequency, int subscriptionid)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            DataTable userIdDt = new DataTable();

            selectQuery += "SELECT site_user_guid FROM subscriptions_purchases WHERE FAIL_COUNT >= 3 AND";
            selectQuery += "group_id" + " IN " + "(" + MCUtils.GetAllGroupIdsToStr(groupID) + ") AND DATEDIFF(DAY, GETDATE(), UPDATE_DATE) = 0" ;
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

        internal override void SetMcObjMergeVars()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += string.Format("SELECT u.ID,u.EMAIL_ADD, u.FIRST_NAME as '*|FNAME|*', sd.description as '*|whichSubscription|*', MAX(sp.UPDATE_DATE) as '*|datePaymentFail|*' FROM Users.dbo.users u, ConditionalAccess.dbo.subscriptions_purchases sp, Pricing.dbo.subscription_descriptions sd WHERE sp.SITE_USER_GUID = u.ID AND sp.SUBSCRIPTION_CODE=sd.subscription_id AND sp.GROUP_ID = {0} AND u.ID IN (1) GROUP BY u.EMAIL_ADD, u.FIRST_NAME, sd.description, u.ID", GroupID, MCUtils.IntListToCsvString(UserGuidList));
            selectQuery.Execute("query", true);
            if (selectQuery.Table("query").DefaultView.Count > 0)
            {
                mcObj.message.merge_vars = new List<MCPerRecipientMergeVars>();
                foreach (DataRow dr in selectQuery.Table("query").Rows)
                {
                    MCPerRecipientMergeVars UserMergeVars = new MCPerRecipientMergeVars() { rcpt = dr["EMAIL_ADD"].ToString() };
                    MCGlobalMergeVars var = new MCGlobalMergeVars();
                    foreach (DataColumn col in selectQuery.Table("query").Columns)
                    {
                        if (col.ColumnName != "EMAIL_ADD" && col.ColumnName != "ID")
                        {
                            var.name = col.ColumnName;
                            var.content = dr[col.ColumnName].ToString();
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
