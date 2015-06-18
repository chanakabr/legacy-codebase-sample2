using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using System.Xml;
using KLogMonitor;

namespace TVinciShared
{
    public class MonitorLogsHelper
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string K_MON_KEY = "kmon";
        private const string PREFIX_UNIQUE_ID = @"urn:uuid:";
        private const string PREFIX_METHOD_NAME = @"urn:Iservice/";

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

        //public static SqlQueryInfo GetSqlDataMonitor(SqlCommand command, ref KLogEnums.eDBQueryType queryType, ref string tableName)
        //{
        //    SqlQueryInfo sqlInfo = new SqlQueryInfo();

        //    if (command != null)
        //    {
        //        if (command.CommandType == System.Data.CommandType.StoredProcedure)
        //        {
        //            sqlInfo.QueryType = KLogEnums.eDBQueryType.EXECUTE;
        //            sqlInfo.Database = command.CommandText != null ? command.CommandText : "null";
        //            sqlInfo.Table = "null";
        //        }
        //        else
        //        {
        //            string query = string.Empty;
        //            if (!string.IsNullOrEmpty(command.CommandText))
        //            {
        //                if (query.StartsWith("select"))
        //                    sqlInfo.QueryType = KLogEnums.eDBQueryType.SELECT;
        //                else if (query.StartsWith("delete"))
        //                    sqlInfo.QueryType = KLogEnums.eDBQueryType.DELETE;
        //                else if (query.StartsWith("insert"))
        //                    sqlInfo.QueryType = KLogEnums.eDBQueryType.INSERT;
        //                else if (query.StartsWith("update"))
        //                    sqlInfo.QueryType = KLogEnums.eDBQueryType.UPDATE;

        //            }
        //            //dataMonitorString = string.Format("{0}:{1}:{2}",
        //            //           command.Connection != null && command.Connection.Database != null ? command.Connection.Database : string.Empty, // 0
        //            //           command.CommandType.ToString(),                                                                                 // 1
        //            //           command.CommandText != null ? command.CommandText : string.Empty);                                              // 2
        //        }
        //    }
        //    return sqlInfo;
        //}

        public static void InitMonitorLogsDataWS(string requestString, string wsName)
        {
            if (!string.IsNullOrEmpty(requestString))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(requestString);

                    // get action name
                    XmlNodeList tempXmlNodeList = doc.GetElementsByTagName("soap:Body");
                    if (tempXmlNodeList.Count > 0)
                        HttpContext.Current.Items.Add(Constants.ACTION, tempXmlNodeList[0].ChildNodes[0].Name);
                    else
                        HttpContext.Current.Items.Add(Constants.ACTION, "null");

                    // get group ID
                    XmlNodeList xmlUserName = doc.GetElementsByTagName("sWSUserName");
                    XmlNodeList xmlPassword = doc.GetElementsByTagName("sWSPassword");
                    if (xmlUserName.Count > 0 && xmlPassword.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(wsName))
                            HttpContext.Current.Items.Add(Constants.GROUP_ID, WS_Utils.GetGroupID(wsName, xmlUserName[0].InnerText, xmlPassword[0].InnerText));
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error while getting log and monitor information", ex);
                }

                // get request ID
                HttpContext.Current.Items.Add(KLogMonitor.Constants.REQUEST_ID_KEY, Guid.NewGuid().ToString());

                // get user agent
                if (HttpContext.Current.Request.UserAgent != null)
                    HttpContext.Current.Items.Add(Constants.CLIENT_TAG, HttpContext.Current.Request.UserAgent);

                // get host IP
                if (HttpContext.Current.Request.UserHostAddress != null)
                    HttpContext.Current.Items.Add(Constants.HOST_IP, HttpContext.Current.Request.UserHostAddress);

                // start k-monitor
                HttpContext.Current.Items.Add(K_MON_KEY, new KMonitor(KLogMonitor.Events.eEvent.EVENT_API_START));

                // log request
                log.Debug(requestString);
            }
        }

        public static void InitMonitorLogsDataWCF(Message requestMessage)
        {
            string requestString = requestMessage.ToString();
            if (!string.IsNullOrEmpty(requestString))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(requestString);

                    // get action name
                    XmlNodeList tempXmlNodeList = doc.GetElementsByTagName("request");
                    if (tempXmlNodeList.Count > 0)
                        OperationContext.Current.IncomingMessageProperties.Add(Constants.ACTION, tempXmlNodeList[0].Attributes["i:type"] != null ? tempXmlNodeList[0].Attributes["i:type"].Value : "null");
                    else
                        OperationContext.Current.IncomingMessageProperties.Add(Constants.ACTION, "null");

                    //// get group ID
                    //XmlNodeList xmlGroupId = doc.GetElementsByTagName("b:m_nGroupID");
                    //if (xmlGroupId.Count > 0)
                    //    OperationContext.Current.IncomingMessageProperties.Add(Constants.GROUP_ID, xmlGroupId[0].InnerText);
                }
                catch (Exception ex)
                {
                    log.Error("Error while getting log and monitor information", ex);
                }

                // get request ID
                if (requestMessage.Headers != null && requestMessage.Headers.MessageId != null)
                    OperationContext.Current.IncomingMessageProperties.Add(KLogMonitor.Constants.REQUEST_ID_KEY, requestMessage.Headers.MessageId.ToString().Replace(PREFIX_UNIQUE_ID, string.Empty));

                // get user agent
                if (HttpContext.Current.Request.UserAgent != null)
                    OperationContext.Current.IncomingMessageProperties.Add(Constants.CLIENT_TAG, Dns.GetHostName());

                // get host IP
                OperationContext.Current.IncomingMessageProperties.Add(Constants.HOST_IP, (requestMessage.Properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address);

                // start k-monitor
                OperationContext.Current.IncomingMessageProperties.Add(K_MON_KEY, new KMonitor(KLogMonitor.Events.eEvent.EVENT_API_START));

                // log request
                log.Debug(requestString);
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
                // ignore exception
            }
        }
    }
}
