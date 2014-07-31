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
                int defaultOperatorId = DAL.UsersDal.GetDefaultGroupOperator(m_nGroupID);
                if (defaultOperatorId == 0)
                {
                    Logger.Logger.Log("Default operatorId is 0","", "GetSSOImplementation error");
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
//User u = new User();

//ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
//selectQuery.SetConnectionKey("users_connection");
//selectQuery += "SELECT u.ID, u.External_Token, udd.Platform from users u, users_dynamic_data udd WHERE u.ID=udd.USER_ID AND";
//selectQuery += ODBCWrapper.Parameter.NEW_PARAM("u.USERNAME", "=", sUN);
//selectQuery += " AND ";
//selectQuery += ODBCWrapper.Parameter.NEW_PARAM("u.GROUP_ID", "=", nGroupID);

//if (selectQuery.Execute("query", true) != null)
//{
//    if (selectQuery.Table("query").DefaultView.Count > 0)
//    {
//        DataRow dr = selectQuery.Table("query").Rows[0];
//        nUserID = int.Parse(dr["ID"].ToString());
//        nUserPlatform = int.Parse(dr["Platform"].ToString());
//        sExternalToken = dr["External_Token"].ToString();
//        u.Initialize(nUserID, nGroupID);
//    }
//}
//selectQuery.Finish();

////Check if the password was changed and if so, updates the DB
//private void CheckIfPasswordNeedsUpdate(string sPass, int siteGuid)
//{
//    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
//    selectQuery += "SELECT password FROM users WHERE";
//    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", siteGuid);
//    if (selectQuery.Execute("query", true) != null)
//    {
//        if (selectQuery.Table("query").DefaultView.Count > 0)
//        {
//            string dbPass = selectQuery.Table("query").Rows[0]["PASSWORD"].ToString();
//            selectQuery.Finish();
//            if (dbPass != sPass)
//            {
//                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users");
//                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PASSWORD", "=", sPass);
//                updateQuery += "WHERE";
//                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", siteGuid);
//                updateQuery.Execute();
//                updateQuery.Finish();
//            }
//        }

//    }
//}
