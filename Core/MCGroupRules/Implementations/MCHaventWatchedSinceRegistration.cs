using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MCGroupRules
{
    public class MCHaventWatchedSinceRegistration : MCImplementationBase
    {

        public MCHaventWatchedSinceRegistration(int groupid, int ruleid, int ruletype, int rulefreq, int subscriptionid)
            : base(groupid, ruleid, ruletype, rulefreq, subscriptionid)
        {

        }

        //Gets all the users that apply to the rule
        public override List<int> GetRuleUsersGuids(int groupID, int ruleType, int ruleInnerFrequency, int subscriptionid)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            DataTable userIdDt = new DataTable();

            selectQuery += string.Format("SELECT u.ID FROM Users.dbo.users u WHERE u.group_id ={0} AND DATEDIFF(DAY, u.CREATE_DATE, GETDATE()) = {1} AND u.ID NOT IN (select site_user_guid FROM users_media_mark WHERE group_id in ({2}))",groupID, MCUtils.GetDaysFromMinPeriodId(ruleInnerFrequency), MCUtils.GetAllGroupIdsToStr(groupID));
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