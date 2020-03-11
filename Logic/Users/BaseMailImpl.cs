using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public abstract class BaseMailImpl
    {
        public int m_nGroupID;
        public int m_nRuleID;

        public BaseMailImpl(int nGroupID, int nRuleID)
        {
            m_nGroupID = nGroupID;
            m_nRuleID = nRuleID;

            if (nRuleID == 0)
            {
                SetDefaultRuleID();
            }
        }

        public abstract bool SendMail(User user);

        public virtual void SetDefaultRuleID()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from groups_mail_rules where status=1 and is_active=1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("Name", "=", "Welcome");
            selectQuery.SetConnectionKey("main_connection");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    object oWelcomeMail = selectQuery.Table("query").DefaultView[0].Row["id"];
                    if (oWelcomeMail != null && oWelcomeMail != DBNull.Value)
                    {
                        m_nRuleID = int.Parse(oWelcomeMail.ToString());
                    }
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }
    }
}
