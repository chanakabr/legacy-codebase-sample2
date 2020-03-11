using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Newtonsoft.Json;
using TVinciShared;

namespace MCGroupRules.Implementations
{
    public class MCWelcomeMail
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int m_nRuleID;
        public int m_nGroupID;

        private MCObjByTemplate mcObj;

        public MCWelcomeMail(int groupid)
        {
            m_nGroupID = groupid;
            m_nRuleID = 0;

            mcObj = null;
        }

        public void InitMCObj(int ruleid, string sEmailAdd, string sFirstName, string sLastName)
        {
            m_nRuleID = ruleid;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "SELECT g.mail_api_key, gmr.GROUP_ID, gmr.Type, gmr.Template_Name, gmr.Mail_From, gmr.Mail_subject ,gmr.min_limit_id FROM groups g, groups_mail_rules gmr WHERE g.ID=gmr.group_id AND";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("gmr.ID", "=", m_nRuleID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("g.ID", "=", m_nGroupID);
            selectQuery.SetConnectionKey("main_connection");
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    mcObj = new MCObjByTemplate();

                    mcObj.key = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "mail_api_key", 0);
                    mcObj.message.subject = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "Mail_subject", 0);
                    mcObj.message.from_email = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "Mail_From", 0);
                    mcObj.message.merge = true;
                    mcObj.template_name = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "Template_Name", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            if (mcObj != null)
            {
                string sName = string.Format("{0} {1}", sFirstName, sLastName);

                SetMcObjTo(sEmailAdd, sName);

                List<MCPerRecipientMergeVars> lmcprmv = new List<MCPerRecipientMergeVars>();

                MCPerRecipientMergeVars mcprmv = new MCPerRecipientMergeVars();
                mcprmv.rcpt = sEmailAdd;

                MCGlobalMergeVars mvFirst = new MCGlobalMergeVars();
                mvFirst.name = "FIRSTNAME";
                mvFirst.content = sFirstName;
                mcprmv.vars.Add(mvFirst);

                MCGlobalMergeVars mvLast = new MCGlobalMergeVars();
                mvLast.name = "LASTNAME";
                mvLast.content = sLastName;

                mcprmv.vars.Add(mvLast);

                lmcprmv.Add(mcprmv);


                mcObj.message.merge_vars = lmcprmv;


            }
        }

        private void SetMcObjTo(string sEmailAdd, string sName)
        {
            MCTo to = new MCTo();

            to.email = sEmailAdd;
            to.name = sName;

            mcObj.message.to.Add(to);
        }

        public bool Send()
        {
            if (mcObj != null)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(mcObj);
                    string sResp = WS_Utils.SendXMLHttpReq("https://mandrillapp.com/api/1.0/messages/send-template.json", json, null);
                    log.InfoFormat("Mail Response: {0}", sResp);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }
    }
}
