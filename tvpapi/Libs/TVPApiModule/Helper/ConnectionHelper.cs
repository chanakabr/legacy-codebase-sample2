using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Collections;
using System.Configuration;
using System.Reflection;
using KLogMonitor;

/// <summary>
/// Summary description for ConnectionHelper
/// </summary>
/// 

namespace TVPApi
{
    public class ConnectionHelper
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private static Dictionary<string, Dictionary<string, int>> _groupsModulesIPs = new Dictionary<string, Dictionary<string, int>>();

        //Get group ID according to ws user name
        static public Int32 GetGroupID(string sWSName, string sModuleName, string sUN, string sPass, string sIP)
        {
            if (_groupsModulesIPs.ContainsKey(sUN + ":" + sPass))
            {
                return GetValueFromDict(sWSName, sModuleName, sUN, sPass, sIP);
            }
            else
            {
                try
                {
                    _locker.EnterWriteLock();

                    //Second check to make sure another thread didn't add the value already
                    if (!_groupsModulesIPs.ContainsKey(sUN + ":" + sPass))
                    {
                        Dictionary<string, int> tmpDict = GetGroupModulesIpsFromDb(sUN, sPass);
                        if (tmpDict != null)
                        {
                            _groupsModulesIPs.Add(sUN + ":" + sPass, tmpDict);
                            return GetValueFromDict(sWSName, sModuleName, sUN, sPass, sIP);
                        }
                        else return 0;
                    }
                    else return GetValueFromDict(sWSName, sModuleName, sUN, sPass, sIP);
                }

                catch (Exception ex)
                {
                    logger.Error("", ex);
                    return 0;
                }

                finally
                {
                    _locker.ExitWriteLock();
                }

            }
        }

        private static int GetValueFromDict(string sWSName, string sModuleName, string sUN, string sPass, string sIP)
        {
            if (_groupsModulesIPs[sUN + ":" + sPass].ContainsKey("00000" + ":" + sWSName + ":" + "00000"))
            {
                return _groupsModulesIPs[sUN + ":" + sPass]["00000" + ":" + sWSName + ":" + "00000"];
            }
            else if (_groupsModulesIPs[sUN + ":" + sPass].ContainsKey("00000" + ":" + sWSName + ":" + sIP))
            {
                return _groupsModulesIPs[sUN + ":" + sPass]["00000" + ":" + sWSName + ":" + sIP];
            }
            else if (_groupsModulesIPs[sUN + ":" + sPass].ContainsKey(sModuleName + ":" + sWSName + ":" + "00000"))
            {
                return _groupsModulesIPs[sUN + ":" + sPass][sModuleName + ":" + sWSName + ":" + "00000"];
            }
            else if (_groupsModulesIPs[sUN + ":" + sPass].ContainsKey(sModuleName + ":" + sWSName + ":" + sIP))
            {
                return _groupsModulesIPs[sUN + ":" + sPass][sModuleName + ":" + sWSName + ":" + sIP];
            }
            else return 0;
        }

        private static Dictionary<string, int> GetGroupModulesIpsFromDb(string sUN, string sPass)
        {
            Dictionary<string, int> retVal = null;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery(GetTvinciConnectionString());

            selectQuery += "select * from groups_modules_ips where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sUN);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PASSWORD", "=", sPass);

            DataTable dt = selectQuery.Execute("query", true);
            if (dt != null)
            {
                Int32 nCount = dt.DefaultView.Count;
                if (nCount > 0)
                {
                    retVal = new Dictionary<string, int>();
                    foreach (DataRow item in dt.Rows)
                    {

                        retVal[item["MODULE_NAME"] + ":" + item["WS_NAME"] + ":" + item["IP"]] =
                            int.Parse(item["GROUP_ID"].ToString());
                    }
                }
            }


            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }

        //Init delegates
        public static void InitServiceConfigs()
        {
            Tvinci.Data.TVMDataLoader.Protocols.Protocol.GetRequestLanguageMethod = GetFlashVarsLangVal;
            Tvinci.Data.TVMDataLoader.TVMProvider.GetTVMUrlMethod = delegate(bool b) { return ConfigurationManager.AppSettings["TVM_API_URL"]; };
        }

        public static string GetFlashVarsLangVal()
        {
            return string.Empty;
        }

        //Get the TVINCI DB connection string
        public static string GetTvinciConnectionString()
        {
            return string.Concat("Driver={SQL Server};Server=", TVinciDBConfiguration.GetConfig().DBServer,
                    ";Database=", TVinciDBConfiguration.GetConfig().DBInstance,
                    ";Uid=", TVinciDBConfiguration.GetConfig().User,
                    ";Pwd=", TVinciDBConfiguration.GetConfig().Pass,
                    ";");
        }

        //Get client specific connection string 
        public static string GetClientConnectionString()
        {
            //try to get groupID of specific request.
            object groupObj = HttpContext.Current.Items["GroupID"];
            PlatformType platform = (PlatformType)Enum.Parse(typeof(PlatformType), HttpContext.Current.Items["Platform"].ToString());
            //Patchy - currently take favorites from Web DB (need service from Guy)
            bool isShared = (bool)HttpContext.Current.Items["IsShared"];
            if (groupObj != null)
            {
                //Get the techinchal manager associated with the current request
                int groupID = (int)groupObj;
                string dbInstance = ConfigManager.GetInstance().GetConfig(groupID, platform).TechnichalConfiguration.Data.DBConfiguration.DatabaseInstance;
                //Patchy - for now take all shared items (like favorites) from Web DB! (Waiting for service from Guy)
                if (isShared)
                {
                    int index = dbInstance.IndexOf(platform.ToString());
                    dbInstance = dbInstance.Substring(0, index - 1);
                }
                //return ConfigManager.GetInstance(groupID).TechnichalConfiguration.GenerateConnectionString();
                return string.Concat("Driver={SQL Server};Server=", ConfigManager.GetInstance().GetConfig(groupID, platform).TechnichalConfiguration.Data.DBConfiguration.IP,
                ";Database=", dbInstance,
                ";Uid=", ConfigManager.GetInstance().GetConfig(groupID, platform).TechnichalConfiguration.Data.DBConfiguration.User,
                ";Pwd=", ConfigManager.GetInstance().GetConfig(groupID, platform).TechnichalConfiguration.Data.DBConfiguration.Pass,
                ";");
            }
            else
            {
                return string.Empty;
            }
        }

        public static Dictionary<string, string> GetSupportedPlatforms()
        {
            Dictionary<string, string> retval = null;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery(GetTvinciConnectionString());

            selectQuery += "select * from lu_platform";

            DataTable dt = selectQuery.Execute("query", true);
            if (dt != null)
            {
                Int32 nCount = dt.DefaultView.Count;
                if (nCount > 0)
                {
                    retval = new Dictionary<string, string>();
                    foreach (DataRow item in dt.Rows)
                    {
                        retval.Add(item["Name"].ToString(), item["ID"].ToString());
                    }
                }
            }

            selectQuery.Finish();
            selectQuery = null;
            return retval;

        }

        public static bool GetApiCredentials(string sCRMUser, string sCRMPass, out string sApiUser, out string sApiPass)
        {
            bool isAuth = false;

            sApiUser = string.Empty;
            sApiPass = string.Empty;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery(GetTvinciConnectionString());

            selectQuery += "select API_USERNAME, API_PASSWORD from crm_users where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CRM_USERNAME", "=", sCRMUser);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CRM_PASSWORD", "=", sCRMPass);

            System.Data.DataTable dt = selectQuery.Execute("query", true);
            if (dt != null)
            {
                Int32 nCount = dt.DefaultView.Count;

                if (nCount > 0)
                {
                    isAuth = true;

                    sApiUser = dt.Rows[0]["API_USERNAME"].ToString();
                    sApiPass = dt.Rows[0]["API_PASSWORD"].ToString();
                }
            }

            selectQuery.Finish();
            selectQuery = null;

            return isAuth;
        }
    }
}
