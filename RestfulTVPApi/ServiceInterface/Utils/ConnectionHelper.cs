using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Collections;
using System.Configuration;
using System.Reflection;
using ServiceStack.Logging;

/// <summary>
/// Summary description for ConnectionHelper
/// </summary>
/// 

namespace RestfulTVPApi.ServiceInterfaces.Utils
{
    public class ConnectionHelper
    {
        #region CONST

        private const string TVINCI_DB_CONFIG = "TVinciDBConfig";

        #endregion

        private static readonly ILog log = LogManager.GetLogger(typeof(ConnectionHelper));
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

            //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery(GetTvinciConnectionString());

            //selectQuery += "select * from groups_modules_ips where is_active=1 and status=1 and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sUN);
            //selectQuery += "and";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PASSWORD", "=", sPass);

            //DataTable dt = selectQuery.Execute("query", true);
            //if (dt != null)
            //{
            //    Int32 nCount = dt.DefaultView.Count;
            //    if (nCount > 0)
            //    {
            //        retVal = new Dictionary<string, int>();
            //        foreach (DataRow item in dt.Rows)
            //        {

            //            retVal[item["MODULE_NAME"] + ":" + item["WS_NAME"] + ":" + item["IP"]] =
            //                int.Parse(item["GROUP_ID"].ToString());
            //        }
            //    }
            //}
            //selectQuery.Finish();
            //selectQuery = null;
            return retVal;
        }

        //Init delegates
        public static void InitServiceConfigs()//int groupID, PlatformType platform
        {
            
            //Tvinci.Data.TVMDataLoader.Protocols.Protocol.GetRequestLanguageMethod = GetFlashVarsLangVal;
            //Tvinci.Data.TVMDataLoader.TVMProvider.GetTVMUrlMethod = delegate(bool b) { return ConfigurationManager.AppSettings["TVM_API_URL"]; };
           
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
        }
    }
}
