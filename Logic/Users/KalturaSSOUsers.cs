using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Data;
using System.Web.Script.Serialization;
using System.IO;
using System.Reflection;
using System.Configuration;
using KLogMonitor;

namespace Core.Users
{
    public class KalturaSSOUsers : KalturaUsers
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());   
        public int OperatorId { get; set; }

        public KalturaSSOUsers(Int32 nGroupID, int operatorId)
            : base(nGroupID)
        {
            OperatorId = operatorId;

            // activate/deactivate user features
            this.ShouldSubscribeNewsLetter = true;
            this.ShouldCreateDefaultRules = true;
            this.ShouldSendWelcomeMail = true;
        }

        public ISSOProvider GetSSOImplementation(int nSSOProvID)
        {
            string key = string.Empty;
            bool bRes;

            // get SSOProviderID
            if (nSSOProvID == 0)
            {
                key = string.Format("users_GetSSOProvID_{0}", GroupId);
                int defaultOperatorId;
                bRes = UsersCache.GetItem<int>(key, out  defaultOperatorId);
                if (!bRes)
                {
                    defaultOperatorId = DAL.UsersDal.GetDefaultGroupOperator(GroupId);
                    if (defaultOperatorId == 0)
                        log.Debug("Default operatorId is 0");
                    else
                        UsersCache.AddItem(key, defaultOperatorId);
                }

                nSSOProvID = defaultOperatorId;
            }


            // get implementation type by groupID + ProviderID
            key = string.Format("users_GetSSOImplementation_{0}_{1}", GroupId, nSSOProvID);
            string moduleName = string.Empty;
            bRes = UsersCache.GetItem<string>(key, out  moduleName);
            if (!bRes)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

                // change to main DB
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");

                selectQuery += "SELECT * FROM groups_operators WHERE STATUS=1 AND IS_ACTIVE=1 AND";
                if (nSSOProvID != 0)
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nSSOProvID);
                else
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("is_default", "=", 1);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", GroupId);

                if (selectQuery.Execute("query", true) != null)
                {
                    DataTable dt = selectQuery.Table("query");
                    if (dt.DefaultView.Count > 0)
                    {
                        moduleName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "module_name", 0);
                        if (!string.IsNullOrEmpty(moduleName))
                            UsersCache.AddItem(key, moduleName);
                    }
                }
                selectQuery.Finish();
            }

            // load user assembly
            string usersAssemblyLocation = Utils.GetTcmConfigValue("USERS_ASSEMBLY_LOCATION");
            Assembly userAssembly = Assembly.LoadFrom(string.Format(@"{0}{1}.dll", usersAssemblyLocation.EndsWith("\\") ? usersAssemblyLocation :
                usersAssemblyLocation + "\\", moduleName));

            // get user class 
            Type userType = userAssembly.GetType(string.Format("{0}.{1}", moduleName, "User"));

            return (ISSOProvider)Activator.CreateInstance(userType, GroupId, nSSOProvID);
        }
    }
}