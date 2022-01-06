using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;
using System.Data;
using Phx.Lib.Log;
using System.Reflection;
using Newtonsoft.Json;

namespace MCGroupRules
{
    public abstract class MCImplementationBase
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public int RuleID { get; internal set; }
        public int GroupID { get; internal set; }
        public int RuleType { get; internal set; }
        public int RulelimitID { get; internal set; }
        public int SubscriptionID { get; internal set; }

        private List<int> _userGuidList;
        public List<int> UserGuidList
        {
            get { return _userGuidList; }
            internal set { _userGuidList = value; FilterUserGuidList(); }
        }

        protected MCObjByTemplate mcObj;

        public MCImplementationBase(int groupid, int ruleid, int ruletype, int limitid, int subscriptionid)
        {
            GroupID = groupid;
            RuleID = ruleid;
            RuleType = ruletype;
            RulelimitID = limitid;
            SubscriptionID = subscriptionid;
            UserGuidList = GetRuleUsersGuids(GroupID, RuleType, RulelimitID, subscriptionid);
        }

        public MCObjByTemplate GetRuleObject()
        {
            InitMCObj();
            return mcObj;
        }

        public bool Send()
        {
            if (mcObj != null)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(mcObj);
                    string sResp = WS_Utils.SendXMLHttpReq("https://mandrillapp.com/api/1.0/messages/send-template.json", json, null);
                    log.DebugFormat("Mail Response: {0}", sResp);

                    MarkRuleAsHandled();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else return false;
        }

        public abstract List<int> GetRuleUsersGuids(int groupID, int ruleType, int ruleInnerFrequency, int subcriptionid);

        private void MarkRuleAsHandled()
        {
            foreach (int guid in UserGuidList)
            {
                ODBCWrapper.DataSetInsertQuery insertQuery = new ODBCWrapper.DataSetInsertQuery("group_rule_uses");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("site_user_guid", guid);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("rule_id", RuleID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", GroupID);

                insertQuery.Execute();
                insertQuery.Finish();
            }
        }

        private void FilterUserGuidList()
        {
            if (UserGuidList.Count > 0)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "SELECT site_user_guid FROM group_rule_uses WHERE ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("RULE_ID", "=", RuleID);
                selectQuery += string.Format("AND site_user_guid IN ({0})", MCUtils.IntListToCsvString(UserGuidList));
                selectQuery.Execute("query", true);

                if (selectQuery.Table("query").DefaultView.Count > 0)
                {
                    DataTable dt = selectQuery.Table("query");
                    selectQuery.Finish();
                    foreach (DataRow guidRow in dt.Rows)
                    {
                        UserGuidList.Remove(int.Parse(guidRow[0].ToString()));
                    }
                }
                selectQuery.Finish();

            }
        }

        //Set MCObject To: and MergeVars
        internal virtual void SetMcObjMergeVars()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            // Get merge vars for rule
            selectQuery += "SELECT Var_Name, Col_Name, Table_Name FROM group_mail_vars where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("rule_id", "=", RuleID);
            selectQuery.Execute("query", true);

            DataTable ruleVarsDt = selectQuery.Table("query");
            selectQuery.Finish();

            if (ruleVarsDt.DefaultView.Count > 0)
            {
                //List of distinct tables to minimize the calls to the DB (one query for multiple vars in single table) 
                List<string> tableNamesList = MCUtils.GetVarTableNames(ruleVarsDt);

                //the return dictionary -  key: email | value : another dictionary with paramName as key and value as value 
                Dictionary<string, Dictionary<string, string>> tmpEmailVarDict = new Dictionary<string, Dictionary<string, string>>();

                //go over each table in the list and create the query for all rule vars in the same table
                foreach (string tableName in tableNamesList)
                {
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();

                    //set the connection string for the right table (Users table is called explicitly to get the right email field)
                    MCUtils.SetConnectionStringByVarTable(tableName, ref selectQuery);
                    selectQuery += "SELECT u.EMAIL_ADD ";
                    Dictionary<string, string> tableVarsDict = MCUtils.GetVarsForTable(ruleVarsDt, tableName);
                    int i = 0;
                    foreach (KeyValuePair<string, string> varRow in tableVarsDict)
                    {
                        i++;
                        selectQuery += ", tblname." + varRow.Key + " as '" + varRow.Value + "'";
                        if (i < tableVarsDict.Count)
                        {
                            selectQuery += ',';
                        }
                    }

                    // get the user-guid column name because many tables have different names for the userID
                    string userGuidColName = MCUtils.GetGuidColNameByTableName(tableName);
                    selectQuery += " FROM Users.dbo.users u, " + tableName + " tblname WHERE u.id=tblname." + userGuidColName + " AND u.ID";

                    // set the sql query with all the users who apply to the rule as a string of CSVs
                    selectQuery += " IN " + "(" + MCUtils.IntListToCsvString(UserGuidList) + ") ORDER BY tblname.UPDATE_DATE DESC";
                    selectQuery.Execute("query", true);
                    DataTable dt = selectQuery.Table("query");
                    selectQuery.Finish();

                    // Adds the vars to the dictionary by the key of the user's email
                    foreach (DataRow mailAndVarsRow in dt.Rows)
                    {
                        string email = mailAndVarsRow["EMAIL_ADD"].ToString();
                        if (!tmpEmailVarDict.ContainsKey(email))
                        {
                            tmpEmailVarDict.Add(email, new Dictionary<string, string>());
                        }

                        foreach (DataColumn dc in dt.Columns)
                        {
                            if (dc.ColumnName != "EMAIL_ADD" && dc.ColumnName != "FIRST_NAME" && dc.ColumnName != "LAST_NAME" && email != string.Empty)
                            {
                                tmpEmailVarDict[email].Add(dc.ColumnName, mailAndVarsRow[dc.ColumnName].ToString());
                            }
                        }
                    }
                }
                // sets the main MC object with the merge vars
                SetMcObjectVarsFromDict(tmpEmailVarDict);
            }
        }


        public void InitMCObj()
        {
            mcObj = new MCObjByTemplate();
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery += "SELECT g.mail_api_key, gmr.GROUP_ID, gmr.Type, gmr.Template_Name, gmr.Mail_From, gmr.Mail_subject ,gmr.min_limit_id FROM groups g, groups_mail_rules gmr WHERE g.ID=gmr.group_id AND";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("gmr.ID", "=", RuleID);
            selectQuery.Execute("query", true);
            DataTable dt = selectQuery.Table("query");
            selectQuery.Finish();

            if (dt.DefaultView.Count > 0)
            {
                mcObj = new MCObjByTemplate();
                int ruleId = RuleID;
                int groupId = int.Parse(dt.Rows[0]["GROUP_ID"].ToString());
                int ruleType = int.Parse(dt.Rows[0]["Type"].ToString());

                if (dt.Rows[0]["min_limit_id"] != DBNull.Value)
                {
                    int minLimitId = int.Parse(dt.Rows[0]["min_limit_id"].ToString());
                }

                mcObj.key = dt.Rows[0]["mail_api_key"].ToString();
                mcObj.message.subject = dt.Rows[0]["Mail_subject"].ToString();
                mcObj.message.from_email = dt.Rows[0]["Mail_From"].ToString();
                mcObj.message.merge = true;
                mcObj.template_name = dt.Rows[0]["Template_Name"].ToString();

                if (UserGuidList.Count > 0)
                {
                    SetMcObjTo();
                    SetMcObjMergeVars();
                }
            }
        }

        public void SetMcObjectVarsFromDict(Dictionary<string, Dictionary<string, string>> varToDict)
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> item in varToDict)
            {
                MCPerRecipientMergeVars mergeVars = new MCPerRecipientMergeVars();
                mergeVars.rcpt = item.Key;
                foreach (KeyValuePair<string, string> content in item.Value)
                {
                    MCGlobalMergeVars perRecipientVar = new MCGlobalMergeVars() { name = content.Key, content = content.Value };
                    mergeVars.vars.Add(perRecipientVar);
                }
                mcObj.message.merge_vars.Add(mergeVars);
            }
        }

        private void SetMcObjTo()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "SELECT EMAIL_ADD, FIRST_NAME, LAST_NAME FROM users WHERE ID IN (" + MCUtils.IntListToCsvString(UserGuidList) + ")";
            selectQuery.SetConnectionKey("users_connection");
            selectQuery.Execute("query", true);
            DataTable dt = selectQuery.Table("query");

            foreach (DataRow ToRow in dt.Rows)
            {
                MCTo to = new MCTo() { email = ToRow["EMAIL_ADD"].ToString(), name = ToRow["FIRST_NAME"].ToString() + " " + ToRow["LAST_NAME"].ToString() };
                mcObj.message.to.Add(to);
            }
        }
    }
}
