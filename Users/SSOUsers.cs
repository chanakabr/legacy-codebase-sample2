using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Data;
using System.Web.Script.Serialization;
using System.IO;

namespace Users
{
    public class SSOUsers : TvinciUsers
    {
        public int OperatorId { get; set; }

        public SSOUsers(Int32 nGroupID, int operatorId)
            : base(nGroupID)
        {
            OperatorId = operatorId;
        }

        public ISSOProvider GetSSOImplementation(int nSSOProvID)
        {
            if (nSSOProvID == 0)
            {
                string key = string.Format("users_GetSSOImplementation_{0}", m_nGroupID);
                int defaultOperatorId;
                bool bRes = UsersCache.GetItem<int>(key, out  defaultOperatorId);
                if (!bRes)
                {
                    defaultOperatorId = DAL.UsersDal.GetDefaultGroupOperator(m_nGroupID);
                    if (defaultOperatorId == 0)
                    {
                        Logger.Logger.Log("Default operatorId is 0", "", "GetSSOImplementation error");
                    }
                    else
                    {
                        UsersCache.AddItem(key, defaultOperatorId);
                    }
                }

                nSSOProvID = defaultOperatorId;
            }

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "SELECT * FROM groups_operators WHERE STATUS=1 AND IS_ACTIVE=1 AND";
            if (nSSOProvID != 0)
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nSSOProvID);
            else
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("is_default", "=", 1);
            selectQuery += " AND ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");

            ISSOProvider impl = null;
            if (selectQuery.Execute("query", true) != null)
            {
                DataTable dt = selectQuery.Table("query");
                if (dt.DefaultView.Count > 0)
                {
                    switch (int.Parse(dt.Rows[0]["Type"].ToString()))
                    {
                        case 1: //Canal
                            return new SSOOAuthImplementation(m_nGroupID, nSSOProvID);
                        case 2: //Ziggo
                            return new SSOOSamlImplementation(m_nGroupID, nSSOProvID);
                        case 3:
                            return new SSOTvinciImplementation(m_nGroupID, nSSOProvID);
                        case 4:
                            return new SSOKdgImplementation(m_nGroupID, nSSOProvID);
                        default:
                            break;
                    }
                }
            }
            selectQuery.Finish();
            return impl;
        }
    }
}