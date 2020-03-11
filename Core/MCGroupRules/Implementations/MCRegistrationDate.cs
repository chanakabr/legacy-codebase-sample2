using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MCGroupRules
{
    public class MCRegistrationDate : MCImplementationBase
    {

        public MCRegistrationDate(int groupid, int ruleid, int ruletype, int rulefreqid, int subscriptionid)
            : base(groupid, ruleid, ruletype, rulefreqid, subscriptionid)
        {

        }

        //Gets all the users that apply to the rule
        public override List<int> GetRuleUsersGuids(int groupID, int ruleType, int ruleInnerFrequency, int subscriptionid)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            DataTable userIdDt = new DataTable();

            selectQuery += string.Format("select ID from users where IS_ACTIVE=1 AND STATUS=1 AND DATEDIFF(DAY,  CREATE_DATE,  GETDATE()) = {0} AND ", MCUtils.GetDaysFromMinPeriodId(ruleInnerFrequency));
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id","=", groupID);
            selectQuery.SetConnectionKey("users_connection");
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
