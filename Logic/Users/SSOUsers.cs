using System;
using System.Data;
using System.Reflection;
using KLogMonitor;

namespace Core.Users
{
    public class SSOUsers : TvinciUsers
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public int OperatorId { get; set; }

        public SSOUsers(Int32 nGroupID, int operatorId)
            : base(nGroupID)
        {
            OperatorId = operatorId;
        }

        public ISSOProvider GetSSOImplementation(int nSSOProvID)
        {
            string key = string.Empty;
            bool bRes;
            #region get SSOProviderID
            if (nSSOProvID == 0)
            {
                key = string.Format("users_GetSSOProvID_{0}", m_nGroupID);
                int defaultOperatorId;
                bRes = UsersCache.GetItem<int>(key, out  defaultOperatorId);
                if (!bRes)
                {
                    defaultOperatorId = DAL.UsersDal.GetDefaultGroupOperator(m_nGroupID);
                    if (defaultOperatorId == 0)
                    {
                        log.Error("Default operatorId is 0");
                    }
                    else
                    {
                        UsersCache.AddItem(key, defaultOperatorId);
                    }
                }

                nSSOProvID = defaultOperatorId;
            }
            #endregion

            #region get implementation type by groupID + ProviderID
            ISSOProvider impl = null;
            key = string.Format("users_GetSSOImplementation_{0}_{1}", m_nGroupID, nSSOProvID);
            int nTypeImp = 0;
            bRes = UsersCache.GetItem<int>(key, out  nTypeImp);
            if (!bRes)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "SELECT * FROM groups_operators WHERE STATUS=1 AND IS_ACTIVE=1 AND";
                if (nSSOProvID != 0)
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nSSOProvID);
                else
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("is_default", "=", 1);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    DataTable dt = selectQuery.Table("query");
                    if (dt.DefaultView.Count > 0)
                    {
                        nTypeImp = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "Type");
                        if (nTypeImp > 0)
                        {
                            UsersCache.AddItem(key, nTypeImp);
                        }
                    }
                }
                selectQuery.Finish();
            }
            #endregion

            switch (nTypeImp)
            {
                case 1: //Canal
                    return new SSOOAuthImplementation(m_nGroupID, nSSOProvID);
                case 2: //Ziggo
                    return new SSOOSamlImplementation(m_nGroupID, nSSOProvID);
                case 3:
                    return new SSOTvinciImplementation(m_nGroupID, nSSOProvID);
                case 4:
                    return new SSOKdgImplementation(m_nGroupID, nSSOProvID);
                case 5:
                    return new SSOMCImplementation(m_nGroupID, nSSOProvID);

                default:
                    break;
            }

            return impl;
        }
    }
}
