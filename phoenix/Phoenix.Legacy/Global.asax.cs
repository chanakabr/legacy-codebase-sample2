using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Routing;
using Phx.Lib.Log;
using WebAPI.App_Start;
using WebAPI.Exceptions;
using WebAPI.Filters;
using Phx.Lib.Appconfig; 
using System.Xml;
using ApiObjects;
using System.Text;

namespace WebAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string FORM_URL_ENCODED = "application/x-www-form-urlencoded";
        private static readonly List<string> ACTIONS_WITHOUT_CREDENTIALS = new List<string>
        {
            "retrieveservicecontent", "updaterecommendationengineconfiguration", "buildiptocountryindex", "updatecache", "getchannelassets",
            "searchassets"
        };

        protected void Application_Start()
        {
            InitializeLogging();
            // This line is here to avoid error while deserilizing json that was serlizied using net core with TypeNameHandling
            TVinciShared.AssemblyUtils.RedirectAssembly("System.Private.CoreLib", "mscorlib");
            // Configuration
            ApplicationConfiguration.Init();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            AutoMapperConfig.RegisterMappings();
            EventNotificationsConfig.SubscribeConsumers();

            KLogMonitor.ConfigurationReloader.LogReloader.GetInstance().Initiate("phoenix_log_configuration"); 
        }

        private static void InitializeLogging()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string assemblyVersion = string.Format("{0}_{1}_{2}", fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart);
            string apiVersion = System.Configuration.ConfigurationManager.AppSettings.Get("apiVersion");
            string logDir = System.Environment.GetEnvironmentVariable("API_LOG_DIR");
            if(logDir != null)
            {
                logDir = System.Environment.ExpandEnvironmentVariables(logDir) + @"\" + apiVersion;
            }
            else
            {
                logDir = @"C:\log\RestfulApi\" + apiVersion;
            }

            log4net.GlobalContext.Properties["LogDir"] = logDir;
            log4net.GlobalContext.Properties["ApiVersion"] = assemblyVersion;

            // build log4net partial file name
            string partialLogName = string.Empty;

            // try to get application name from virtual path
            string ApplicationAlias = HostingEnvironment.ApplicationVirtualPath;
            if (ApplicationAlias.Length > 2)
                partialLogName = ApplicationAlias.Substring(1);
            else
            {
                // try to get application name from application ID
                string applicationID = HostingEnvironment.ApplicationID;
                if (!string.IsNullOrEmpty(applicationID))
                {
                    var appIdArr = applicationID.Split('/');
                    if (appIdArr != null && appIdArr.Length > 0)
                        partialLogName = appIdArr[appIdArr.Length - 1];
                }
            }

            if (string.IsNullOrWhiteSpace(partialLogName))
            {
                // error getting application name - invent a log name
                partialLogName = Guid.NewGuid().ToString();
            }

            log4net.GlobalContext.Properties["LogName"] = partialLogName;

            // set monitor and log configuration files
            KMonitor.Configure("log4net.config", KLogEnums.AppType.WS);
            KLogger.Configure("log4net.config", KLogEnums.AppType.WS);
        }

        private static Int32 GetGroupID(string requestString)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(requestString);

            // get action name
            var actionName = string.Empty;
            XmlNodeList tempXmlNodeList = doc.GetElementsByTagName("soap:Body");
            if (tempXmlNodeList.Count > 0)
            {
                actionName = tempXmlNodeList[0].ChildNodes[0].Name;
                HttpContext.Current.Items[Constants.ACTION] = tempXmlNodeList[0].ChildNodes[0].Name;
            }

            // get group ID
            XmlNodeList xmlUserName = doc.GetElementsByTagName("sWSUserName");
            XmlNodeList xmlPassword = doc.GetElementsByTagName("sWSPassword");

            if (xmlUserName == null || xmlUserName.Count == 0)
            {
                xmlUserName = doc.GetElementsByTagName("userName");
                if (xmlUserName == null || xmlUserName.Count == 0)
                {
                    xmlUserName = doc.GetElementsByTagName("username");
                }
            }

            if (xmlPassword == null || xmlPassword.Count == 0)
                xmlPassword = doc.GetElementsByTagName("password");
            
            string module = "USERS";

            var isValid = true;
            if (xmlUserName != null && xmlUserName.Count > 0)
            {
                module = GetWsModuleByUserName(xmlUserName[0].InnerText);
            }
            else
            {
                if(!ACTIONS_WITHOUT_CREDENTIALS.Contains(actionName.ToLower()))
                    log.Error($"sWSUserName was not found in XML request. Request: {requestString}");
                isValid = false;
            }

            if (xmlPassword != null && xmlPassword.Count == 0)
            {
                if(!ACTIONS_WITHOUT_CREDENTIALS.Contains(actionName.ToLower()))
                    log.Error($"sWSPassword was not found in XML request. Request: {requestString}");
                isValid = false;
            }

            try
            {
                // no point of trying to get group id if xml is not valid
                if (isValid)
                {
                    Credentials wsc = new Credentials(xmlUserName[0].InnerText, xmlPassword[0].InnerText);
                    return TvinciCache.WSCredentials.GetGroupID(module, wsc);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to get groupId", ex);
            }

            return 0;
        }

        static private int GetGroupIdFormUrlEncoded(string requestString)
        {
            int groupId = 0;
            try
            {
                NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(requestString);
                string username = nameValueCollection["sWSUserName"];
                string module = GetWsModuleByUserName(username);
                string password = nameValueCollection["sWSPassword"];
                Credentials wsc = new Credentials(username, password);
                groupId = TvinciCache.WSCredentials.GetGroupID(module, wsc);                
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to get groupId from FormUrlEncoded", ex);
            }

            return groupId;
        }

        private static string GetWsModuleByUserName(string userName)
        {
            string[] splitedUserName = userName.Split('_');
            if (splitedUserName != null && splitedUserName.Length > 0 && !string.IsNullOrEmpty(splitedUserName[0]))
            {
                return splitedUserName[0];
            }

            return "USERS";
        }

        protected void Application_BeginRequest()
        {
            KMonitor.SetAppType(KLogEnums.AppType.WS);
            KLogger.SetAppType(KLogEnums.AppType.WS);
            if (!Request.AppRelativeCurrentExecutionFilePath.ToLower().Contains("/api_v"))
            {
                // set appType to WCF only if we know its a WCF service
                if (Request.AppRelativeCurrentExecutionFilePath.ToLower().Contains(".svc"))
                {
                    KMonitor.SetAppType(KLogEnums.AppType.WCF);
                    KLogger.SetAppType(KLogEnums.AppType.WCF);
                }

                // initialize monitor and logs parameters
                string requestString = GetWebServiceRequestString();
                if (!string.IsNullOrEmpty(requestString))
                {
                    if (requestString.ToLower().Contains("<soap"))
                    {
                        // soap request
                        int groupId = GetGroupID(requestString);
                        MonitorLogsHelper.InitMonitorLogsDataWs("USERS", requestString, groupId);
                    }
                    else if (Request.ContentType == FORM_URL_ENCODED)
                    {
                        string action = string.Empty;
                        if (!string.IsNullOrEmpty(Request.PathInfo) && Request.PathInfo.Length > 1)
                        {
                            action = Request.PathInfo.Substring(1);
                        }

                        int groupId = GetGroupIdFormUrlEncoded(requestString);
                        MonitorLogsHelper.InitMonitorLogsDataFormUrlEncoded(action, requestString, groupId);
                    }
                }
            }
            else
            {
                // get host IP
                if (HttpContext.Current.Request.UserHostAddress != null)
                    HttpContext.Current.Items[Constants.HOST_IP] = HttpContext.Current.Request.UserHostAddress;

            }
        }

        private static string GetWebServiceRequestString()
        {
            try
            {
                var req = HttpContext.Current?.Request;
                if (req != null)
                {
                    // create byte array to hold request bytes
                    byte[] inputStream = new byte[req.ContentLength];

                    // read entire request input stream
                    req.InputStream.Read(inputStream, 0, inputStream.Length);

                    // set stream back to beginning
                    req.InputStream.Position = 0;

                    // get request string
                    return ASCIIEncoding.ASCII.GetString(inputStream);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to get request string", ex);
            }

            return null;
        }
    }
}
