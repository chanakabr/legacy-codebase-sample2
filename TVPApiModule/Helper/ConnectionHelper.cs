using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Collections;
using log4net;
using System.Configuration;
using System.Reflection;
using TVPApiModule.DBConfiguration;
using TVPApiModule.Context;
using TVPApiModule.Manager;

/// <summary>
/// Summary description for ConnectionHelper
/// </summary>
/// 

namespace TVPApiModule.Helper
{
    public class ConnectionHelper
    {
        #region CONST

        private const string TVINCI_DB_CONFIG = "TVinciDBConfig";

        #endregion

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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
                    log.Error(ex);
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
        public static void InitServiceConfigs()//int groupID, PlatformType platform
        {
            //ConnectionManager connMngr = new ConnectionManager(groupID, platform, false);
            //ODBCWrapper.Connection.GetDefaultConnectionStringMethod = connMngr.GetClientConnectionString;
            //string EnvironmentClient = System.Configuration.ConfigurationManager.AppSettings["ClientIndentifier"].ToLower();

            //if (!string.IsNullOrEmpty(ConfigManager.GetInstance(groupID, platform.ToString()).TechnichalConfiguration.Data.Site.LogBasePath))
            //{
            //    log4net.GlobalContext.Properties["DebuggingLogFilePath"] = string.Format(@"{0}\{1}\Debugging_{2}.log", ConfigManager.GetInstance(groupID, platform.ToString()).TechnichalConfiguration.Data.Site.LogBasePath, EnvironmentClient, System.Environment.MachineName);
            //    log4net.GlobalContext.Properties["InformationLogFilePath"] = string.Format(@"{0}\{1}\Information_{2}.log", ConfigManager.GetInstance(groupID, platform.ToString()).TechnichalConfiguration.Data.Site.LogBasePath, EnvironmentClient, System.Environment.MachineName);
            //    log4net.GlobalContext.Properties["ExceptionsLogFilePath"] = string.Format(@"{0}\{1}\Exceptions_{2}.log", ConfigManager.GetInstance(groupID, platform.ToString()).TechnichalConfiguration.Data.Site.LogBasePath, EnvironmentClient, System.Environment.MachineName);
            //    log4net.GlobalContext.Properties["PerformancesLogFilePath"] = string.Format(@"{0}\{1}\Performances_{2}.log", ConfigManager.GetInstance(groupID, platform.ToString()).TechnichalConfiguration.Data.Site.LogBasePath, EnvironmentClient, System.Environment.MachineName);

            //    string logConfigPath = System.Configuration.ConfigurationManager.AppSettings["Log4NetConfiguration"];
            //    if (!string.IsNullOrEmpty(logConfigPath))
            //    {
            //        logConfigPath = HttpContext.Current.Server.MapPath(logConfigPath);
            //        log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(logConfigPath));
            //    }
            //}

            //Tvinci.Data.TVMDataLoader.Protocols.Protocol.GetTVMConfigurationMethod = delegate() { return ConfigManager.GetInstance().GetConfig(groupID, platform).TechnichalConfiguration.TVMConfiguration; };
            //TVPPro.SiteManager.Manager.TechnicalManager.GetTVMConfiguration;
            Tvinci.Data.TVMDataLoader.Protocols.Protocol.GetRequestLanguageMethod = GetFlashVarsLangVal;
            Tvinci.Data.TVMDataLoader.TVMProvider.GetTVMUrlMethod = delegate(bool b) { return ConfigurationManager.AppSettings["TVM_API_URL"]; };
            //TVPPro.SiteManager.Manager.TextLocalization.Instance.Dispose();
            //TVPPro.SiteManager.Manager.TextLocalization.Instance.TranslationCulture = HttpContext.Current.Items["GroupID"].ToString();
            // TVPPro.SiteManager.Manager.TextLocalization.Instance.Sync(null);

            //  TVPPro.SiteManager.Manager.TextLocalization.Instance.RegisterInstance();
        }

        public static string GetFlashVarsLangVal()
        {
            return string.Empty;
        }

        //Get the TVINCI DB connection string
        public static string GetTvinciConnectionString()
        {
            string sConnectionString = string.Empty;
            try
            {
                string dbServer = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}.{1}", TVINCI_DB_CONFIG, "DBServer"));
                string dbInstance = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}.{1}", TVINCI_DB_CONFIG, "DBInstance"));
                string user = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}.{1}", TVINCI_DB_CONFIG, "User"));
                string pass = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}.{1}", TVINCI_DB_CONFIG, "Pass"));

                sConnectionString = string.Concat("Driver={SQL Server};Server=", dbServer, ";Database=", dbInstance, ";Uid=", user, ";Pwd=", pass, ";");
            }
            catch (Exception ex)
            {
                // Write log here
            }

            return sConnectionString;
            //return string.Concat("Driver={SQL Server};Server=", TVinciDBConfiguration.GetConfig().DBServer,
            //        ";Database=", TVinciDBConfiguration.GetConfig().DBInstance,
            //        ";Uid=", TVinciDBConfiguration.GetConfig().User,
            //        ";Pwd=", TVinciDBConfiguration.GetConfig().Pass,
            //        ";");
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
