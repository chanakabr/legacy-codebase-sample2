using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using log4net;
using System.Configuration;

/// <summary>
/// Summary description for ConnectionHelper
/// </summary>
/// 

namespace TVPApi
{
    public class ConnectionHelper
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ConnectionHelper));

        public ConnectionHelper()
        {

        }

        //Get group ID according to ws user name
        static public Int32 GetGroupID(string sWSName, string sModuleName, string sUN, string sPass, string sIP)
        {
            try
            {
                Int32 nGroupID = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery(GetTvinciConnectionString());
                
                selectQuery += "select group_id from groups_modules_ips where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sUN);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PASSWORD", "=", sPass);
                selectQuery += "and (";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_NAME", "=", sModuleName);
                selectQuery += " or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_NAME", "=", "00000");
                selectQuery += ") and (";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP", "=", sIP);
                selectQuery += " or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP", "=", "00000");
                selectQuery += ") and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WS_NAME", "=", sWSName);
                selectQuery += "order by MODULE_NAME desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
                return nGroupID;
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("GetGroupID-> Params:[sWSName: {0}, sModuleName: {1}, sUN: {2}, sPass: {3}, sIP: {4}]", sWSName, sModuleName, sUN, sPass, sIP), ex);
            }
            return 0;
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
    }
}
