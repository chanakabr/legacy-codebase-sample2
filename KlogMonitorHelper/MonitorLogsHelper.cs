using System;
using System.Collections;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using System.Xml;
using ApiObjects;
using KLogMonitor;

namespace KlogMonitorHelper
{
    public class MonitorLogsHelper
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string K_MON_KEY = "kmon";
        private const string PREFIX_UNIQUE_ID = @"urn:uuid:";
        private const string PREFIX_METHOD_NAME = @"urn:Iservice/";
        private const int MAX_LOG_REQUEST_SIZE = 30000;

        public static string GetWebServiceRequestString()
        {
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    // create byte array to hold request bytes
                    byte[] inputStream = new byte[HttpContext.Current.Request.ContentLength];

                    // read entire request input stream
                    HttpContext.Current.Request.InputStream.Read(inputStream, 0, inputStream.Length);

                    // set stream back to beginning
                    HttpContext.Current.Request.InputStream.Position = 0;

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

        public static void InitMonitorLogsDataWS(eWSModules module, string requestString)
        {
            KLogger.AppType = KLogEnums.AppType.WS;
            KMonitor.AppType = KLogEnums.AppType.WS;

            if (string.IsNullOrEmpty(requestString))
                log.Debug("REQUEST STRING IS EMPTY");
            else
            {
                // get request ID
                if (HttpContext.Current.Request.Headers[KLogMonitor.Constants.REQUEST_ID_KEY.ToString()] == null)
                    HttpContext.Current.Items[KLogMonitor.Constants.REQUEST_ID_KEY] = Guid.NewGuid().ToString();
                else
                    HttpContext.Current.Items[KLogMonitor.Constants.REQUEST_ID_KEY] = HttpContext.Current.Request.Headers[KLogMonitor.Constants.REQUEST_ID_KEY];

                // get user agent
                if (HttpContext.Current.Request.UserAgent != null)
                    HttpContext.Current.Items[Constants.CLIENT_TAG] = HttpContext.Current.Request.UserAgent;

                // get host IP
                if (HttpContext.Current.Request.UserHostAddress != null)
                    HttpContext.Current.Items[Constants.HOST_IP] = HttpContext.Current.Request.UserHostAddress;

                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(requestString);

                    // get action name
                    XmlNodeList tempXmlNodeList = doc.GetElementsByTagName("soap:Body");
                    if (tempXmlNodeList.Count > 0)
                        HttpContext.Current.Items[Constants.ACTION] = tempXmlNodeList[0].ChildNodes[0].Name;
                    else
                        HttpContext.Current.Items[Constants.ACTION] = "null";

                    // get group ID
                    XmlNodeList xmlUserName = doc.GetElementsByTagName("sWSUserName");
                    XmlNodeList xmlPassword = doc.GetElementsByTagName("sWSPassword");
                    if (xmlUserName.Count > 0 && xmlPassword.Count > 0)
                        HttpContext.Current.Items[Constants.GROUP_ID] = GetGroupID(module, xmlUserName[0].InnerText, xmlPassword[0].InnerText);
                }
                catch (Exception)
                {
                    // no need to log exception
                    log.Error(string.Format("Error while loading and parsing WS XML request. XML Request: {0}", requestString));

                    // try taking data from query string
                    try
                    {
                        var nameValueCollection = HttpUtility.ParseQueryString(requestString);
                        string username = nameValueCollection["sWSUserName"];
                        string password = nameValueCollection["sWSPassword"];
                        HttpContext.Current.Items[Constants.GROUP_ID] = GetGroupID(module, username, password);
                    }
                    catch (Exception)
                    {
                        // no need to log exception
                        log.Error(string.Format("Error while loading and parsing WS query string. XML Request: {0}", requestString));
                    }
                }

                // start k-monitor
                HttpContext.Current.Items[K_MON_KEY] = new KMonitor(KLogMonitor.Events.eEvent.EVENT_API_START);

                // log request
                if (requestString.Length > MAX_LOG_REQUEST_SIZE)
                    log.Debug("REQUEST STRING (large request string - partial log): " + requestString.Substring(0, MAX_LOG_REQUEST_SIZE));
                else
                    log.Debug("REQUEST STRING: " + requestString);
            }
        }

        public static void InitMonitorLogsDataWCF(Message requestMessage)
        {
            KLogger.AppType = KLogEnums.AppType.WCF;
            KMonitor.AppType = KLogEnums.AppType.WCF;

            if (requestMessage == null)
                log.Debug("REQUEST STRING IS NULL");
            else
            {
                string requestString = requestMessage.ToString();

                if (string.IsNullOrEmpty(requestString))
                    log.Debug("REQUEST STRING IS EMPTY");
                else
                {
                    if (requestMessage.Headers != null)
                    {
                        // get action name
                        if (requestMessage.Headers.Action != null)
                        {
                            string actionName = requestMessage.Headers.Action.Substring(requestMessage.Headers.Action.LastIndexOf("/") + 1);
                            OperationContext.Current.IncomingMessageProperties[Constants.ACTION] = actionName;
                        }
                        else
                            OperationContext.Current.IncomingMessageProperties[Constants.ACTION] = "null";
                    }

                    // get request ID
                    if (OperationContext.Current.IncomingMessageHeaders.FindHeader(KLogMonitor.Constants.REQUEST_ID_KEY, string.Empty) == -1)
                        OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.REQUEST_ID_KEY] = Guid.NewGuid().ToString();
                    else
                        OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.REQUEST_ID_KEY] = OperationContext.Current.IncomingMessageHeaders.GetHeader<string>(KLogMonitor.Constants.REQUEST_ID_KEY, string.Empty);

                    // get user agent
                    OperationContext.Current.IncomingMessageProperties[Constants.CLIENT_TAG] = Dns.GetHostName();

                    // get host IP
                    if (requestMessage.Properties != null && requestMessage.Properties[RemoteEndpointMessageProperty.Name] != null)
                        OperationContext.Current.IncomingMessageProperties[Constants.HOST_IP] = (requestMessage.Properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address;

                    // start k-monitor
                    OperationContext.Current.IncomingMessageProperties[K_MON_KEY] = new KMonitor(KLogMonitor.Events.eEvent.EVENT_API_START);

                    // log request
                    if (requestString.Length > MAX_LOG_REQUEST_SIZE)
                        log.Debug("REQUEST STRING (large request string - partial log): " + requestString.Substring(0, MAX_LOG_REQUEST_SIZE));
                    else
                        log.Debug("REQUEST STRING: " + requestString);
                }
            }
        }

        public static void FinalizeMonitorLogsData(KLogMonitor.KLogEnums.AppType appType)
        {
            try
            {
                switch (appType)
                {
                    case KLogEnums.AppType.WCF:

                        object temp;
                        if (OperationContext.Current.IncomingMessageProperties.TryGetValue(K_MON_KEY, out temp))
                        {
                            // dispose monitor
                            (temp as KMonitor).Dispose();

                            // remove monitor object
                            OperationContext.Current.IncomingMessageProperties.Remove(K_MON_KEY);
                        }
                        break;

                    case KLogEnums.AppType.WS:
                    default:

                        if (HttpContext.Current.Items[K_MON_KEY] != null)
                        {
                            // dispose monitor
                            (HttpContext.Current.Items[K_MON_KEY] as KMonitor).Dispose();

                            // remove monitor object
                            HttpContext.Current.Items.Remove(K_MON_KEY);
                        }
                        break;
                }
            }
            catch (Exception)
            {
                // ignore error - this will happen when updating WCF reference - NO NEED TO LOG
                //log.Error("Error while trying to dispose monitor object", ex);
            }
        }

        static private Int32 GetGroupID(eWSModules module, string sWSUserName, string sWSPassword)
        {
            try
            {
                Credentials wsc = new Credentials(sWSUserName, sWSPassword);
                return TvinciCache.WSCredentials.GetGroupID(module, wsc);
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to get groupId", ex);
            }
            return 0;
        }

        public static void AddHeaderToWebService(HttpWebRequest request)
        {
            if (KLogMonitor.KLogger.AppType == KLogEnums.AppType.WCF)
            {
                if (request.Headers != null &&
                request.Headers[KLogMonitor.Constants.REQUEST_ID_KEY] == null &&
                OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.REQUEST_ID_KEY] != null)
                {
                    request.Headers.Add(KLogMonitor.Constants.REQUEST_ID_KEY, OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.REQUEST_ID_KEY].ToString());
                }
            }
            else
            {
                if (request.Headers != null &&
                request.Headers[KLogMonitor.Constants.REQUEST_ID_KEY] == null &&
                HttpContext.Current.Items[KLogMonitor.Constants.REQUEST_ID_KEY] != null)
                {
                    request.Headers.Add(KLogMonitor.Constants.REQUEST_ID_KEY, HttpContext.Current.Items[KLogMonitor.Constants.REQUEST_ID_KEY].ToString());
                }
            }
        }
    }
}
