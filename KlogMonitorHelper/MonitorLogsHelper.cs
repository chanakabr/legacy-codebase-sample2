using System;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using System.Xml;
using KLogMonitor;
using log4net.Util;



namespace KlogMonitorHelper
{
    public class MonitorLogsHelper
    {
        #if NET452
        private static readonly HttpRequest _CurrentRequest = HttpContext.Current?.Request;
        #endif

        private static readonly LogicalThreadContextProperties _LogContextData = log4net.LogicalThreadContext.Properties;

        private static readonly KLogger _Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string K_MON_KEY = "kmon";
        private const string PREFIX_UNIQUE_ID = @"urn:uuid:";
        private const string PREFIX_METHOD_NAME = @"urn:Iservice/";
        private const int MAX_LOG_REQUEST_SIZE = 30000;

         #if NET452

        // TODO: Move this to a different helper, this is not related to logs its simply reads the body of an http request
        public static string GetWebServiceRequestString()
        {
            try
            {
                if (_CurrentRequest != null)
                {
                    // create byte array to hold request bytes
                    byte[] inputStream = new byte[_CurrentRequest.ContentLength];

                    // read entire request input stream
                    _CurrentRequest.InputStream.Read(inputStream, 0, inputStream.Length);

                    // set stream back to beginning
                    _CurrentRequest.InputStream.Position = 0;

                    // get request string
                    return ASCIIEncoding.ASCII.GetString(inputStream);
                }
            }
            catch (Exception ex)
            {
                _Log.Error("Error while trying to get request string", ex);
            }

            return null;
        }

        public static void InitMonitorLogsDataWS(string module, string requestBody, int groupId)
        {
            KLogger.AppType = KLogEnums.AppType.WS;
            KMonitor.AppType = KLogEnums.AppType.WS;

            if (string.IsNullOrEmpty(requestBody))
                _Log.Debug("REQUEST BODY IS EMPTY");
            else
            {
                // get request ID
                if (_CurrentRequest.Headers[Constants.REQUEST_ID_KEY] == null)
                    _LogContextData[Constants.REQUEST_ID_KEY] = Guid.NewGuid().ToString();
                else
                    _LogContextData[Constants.REQUEST_ID_KEY] = _CurrentRequest.Headers[Constants.REQUEST_ID_KEY];

                if (_CurrentRequest.UserAgent != null)
                    _LogContextData[Constants.CLIENT_TAG] = _CurrentRequest.UserAgent;

                if (_CurrentRequest.UserHostAddress != null)
                    _LogContextData[Constants.HOST_IP] = _CurrentRequest.UserHostAddress;

                try
                {
                    _LogContextData[Constants.ACTION] = "null";

                    var doc = new XmlDocument();
                    doc.LoadXml(requestBody);

                    // get action name
                    var tempXmlNodeList = doc.GetElementsByTagName("soap:Body");
                    if (tempXmlNodeList.Count > 0)
                        _LogContextData[Constants.ACTION] = tempXmlNodeList[0].ChildNodes[0].Name;
                    
                    _LogContextData[Constants.GROUP_ID] = groupId;
                }
                catch (Exception)
                {
                    // no need to log exception
                    _Log.Error(string.Format("Error while loading and parsing WS XML request. XML Request: {0}", requestBody));

                    // try taking data from query string
                    try
                    {
                        var nameValueCollection = HttpUtility.ParseQueryString(requestBody);
                        _LogContextData[Constants.GROUP_ID] = groupId;

                        if (_CurrentRequest.UrlReferrer != null &&
                            string.IsNullOrEmpty(_CurrentRequest.UrlReferrer.Query))
                        {
                            _LogContextData[Constants.ACTION] = _CurrentRequest.UrlReferrer.Query.Substring(_CurrentRequest.UrlReferrer.Query.LastIndexOf("=") + 1);
                        }
                    }
                    catch (Exception)
                    {
                        // no need to log exception
                        _Log.Error(string.Format("Error while loading and parsing WS query string. XML Request: {0}", requestBody));
                    }
                }

                // start k-monitor
                _LogContextData[K_MON_KEY] = new KMonitor(KLogMonitor.Events.eEvent.EVENT_API_START);

                // log request
                if (requestBody.Length > MAX_LOG_REQUEST_SIZE)
                    _Log.Debug("REQUEST STRING (large request string - partial log): " + requestBody.Substring(0, MAX_LOG_REQUEST_SIZE));
                else
                    _Log.Debug("REQUEST STRING: " + Environment.NewLine + requestBody);
            }
        }

        public static void InitMonitorLogsDataFormUrlEncoded(string action, string requestString, int groupId)
        {
            if (string.IsNullOrEmpty(requestString))
                _Log.Debug("REQUEST STRING IS EMPTY");
            else
            {
                try
                {
                    _LogContextData[Constants.GROUP_ID] = groupId;

                    // get request ID
                    if (_CurrentRequest.Headers[KLogMonitor.Constants.REQUEST_ID_KEY.ToString()] == null)
                        _LogContextData[KLogMonitor.Constants.REQUEST_ID_KEY] = Guid.NewGuid().ToString();
                    else
                        _LogContextData[KLogMonitor.Constants.REQUEST_ID_KEY] = _CurrentRequest.Headers[KLogMonitor.Constants.REQUEST_ID_KEY];

                    // get user agent
                    if (_CurrentRequest.UserAgent != null)
                        _LogContextData[Constants.CLIENT_TAG] = _CurrentRequest.UserAgent;

                    // get host IP
                    if (_CurrentRequest.UserHostAddress != null)
                        _LogContextData[Constants.HOST_IP] = _CurrentRequest.UserHostAddress;


                    _LogContextData[Constants.ACTION] = action;
                    var nameValueCollection = HttpUtility.ParseQueryString(requestString);

                    // start k-monitor
                    _LogContextData[K_MON_KEY] = new KMonitor(KLogMonitor.Events.eEvent.EVENT_API_START);                    

                    // log request
                    if (requestString.Length > MAX_LOG_REQUEST_SIZE)
                        _Log.Debug("REQUEST STRING (large request string - partial log): " + requestString.Substring(0, MAX_LOG_REQUEST_SIZE));
                    else
                        _Log.Debug("REQUEST STRING: " + Environment.NewLine + requestString);
                }
                catch (Exception ex)
                {
                    _Log.Error("Error while loading and parsing data form", ex);
                }
            }
        }

        public static void InitMonitorLogsDataWCF(Message requestMessage)
        {
            KLogger.AppType = KLogEnums.AppType.WCF;
            KMonitor.AppType = KLogEnums.AppType.WCF;

            if (requestMessage == null)
                _Log.Debug("REQUEST STRING IS NULL");
            else
            {
                string requestString = requestMessage.ToString();

                if (string.IsNullOrEmpty(requestString))
                    _Log.Debug("REQUEST STRING IS EMPTY");
                else
                {
                    // get action name
                    if (requestMessage.Headers != null)
                    {
                        if (requestMessage.Headers.Action != null)
                        {
                            string actionName = requestMessage.Headers.Action.Substring(requestMessage.Headers.Action.LastIndexOf("/") + 1);
                            MonitorLogsHelper.SetContext(Constants.ACTION, actionName);
                        }
                    }
                    else
                        MonitorLogsHelper.SetContext(Constants.ACTION, "null");

                    // get request ID
                    if (OperationContext.Current.IncomingMessageHeaders.FindHeader(KLogMonitor.Constants.REQUEST_ID_KEY, string.Empty) == -1)
                        MonitorLogsHelper.SetContext(KLogMonitor.Constants.REQUEST_ID_KEY, Guid.NewGuid().ToString());
                    else
                        MonitorLogsHelper.SetContext(KLogMonitor.Constants.REQUEST_ID_KEY, OperationContext.Current.IncomingMessageHeaders.GetHeader<string>(KLogMonitor.Constants.REQUEST_ID_KEY, string.Empty));

                    // get KS
                    // Removed from log due to cluttering and it was not really used.
                    //if (OperationContext.Current.IncomingMessageHeaders.FindHeader(KLogMonitor.Constants.KS, string.Empty) == -1)
                    //    MonitorLogsHelper.SetContext(KLogMonitor.Constants.KS, Guid.NewGuid().ToString());
                    //else
                    //    MonitorLogsHelper.SetContext(KLogMonitor.Constants.KS, OperationContext.Current.IncomingMessageHeaders.GetHeader<string>(KLogMonitor.Constants.KS, string.Empty));

                    // get user agent
                    MonitorLogsHelper.SetContext(Constants.CLIENT_TAG, Dns.GetHostName());

                    // get host IP
                    if (requestMessage.Properties != null && requestMessage.Properties[RemoteEndpointMessageProperty.Name] != null)
                        MonitorLogsHelper.SetContext(Constants.HOST_IP, (requestMessage.Properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address);

                    // start k-monitor
                    MonitorLogsHelper.SetContext(K_MON_KEY, new KMonitor(KLogMonitor.Events.eEvent.EVENT_API_START));

                    // log request
                    if (requestString.Length > MAX_LOG_REQUEST_SIZE)
                        _Log.Debug("REQUEST STRING (large request string - partial log): " + requestString.Substring(0, MAX_LOG_REQUEST_SIZE));
                    else
                        _Log.Debug("REQUEST STRING: " + Environment.NewLine + requestString);
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

                        if (_LogContextData[K_MON_KEY] != null)
                        {
                            // dispose monitor
                            (_LogContextData[K_MON_KEY] as KMonitor).Dispose();

                            // remove monitor object
                            _LogContextData.Remove(K_MON_KEY);
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
        #endif

        public static void AddHeaderToWebService(HttpWebRequest request)
        {
            if (KLogMonitor.KLogger.AppType == KLogEnums.AppType.WCF)
            {
                object res = null;
                if (request.Headers != null &&
                request.Headers[KLogMonitor.Constants.REQUEST_ID_KEY] == null &&
                OperationContext.Current != null &&
                OperationContext.Current.IncomingMessageProperties != null &&
                !OperationContext.Current.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.REQUEST_ID_KEY, out res))
                {
                    request.Headers.Add(KLogMonitor.Constants.REQUEST_ID_KEY, OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.REQUEST_ID_KEY].ToString());
                }
            }
            else
            {
                if (request.Headers != null &&
                request.Headers[KLogMonitor.Constants.REQUEST_ID_KEY] == null &&
                HttpContext.Current != null &&
                _LogContextData != null &&
                _LogContextData[KLogMonitor.Constants.REQUEST_ID_KEY] != null)
                {
                    request.Headers.Add(KLogMonitor.Constants.REQUEST_ID_KEY, _LogContextData[KLogMonitor.Constants.REQUEST_ID_KEY].ToString());
                }
            }
        }

        public static void SetContext(string key, object value)
        {
            _LogContextData[key] = value;

            if (OperationContext.Current != null)
            {
                OperationContext.Current.IncomingMessageProperties[key] = value;
            }
        }
        
        public static bool UpdateHeaderData(string key, string value)
        {
            try
            {
                KMonitor.LogContextData[key] = value;
                return true;
            }
            catch (Exception ex)
            {
                _Log.ErrorFormat("Error while trying to update header key. app type: {0}, key: {1}, value: {2}, ex: {3}", KMonitor.AppType.ToString(), key, value, ex);
            }

            return false;
        }
    }
}