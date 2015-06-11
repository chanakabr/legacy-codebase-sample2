using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        

        public static void InitMonitorLogsData(out string requestString)
        {
            // get XML soap request

            // create byte array to hold request bytes
            byte[] inputStream = new byte[HttpContext.Current.Request.ContentLength];

            // read entire request input stream
            HttpContext.Current.Request.InputStream.Read(inputStream, 0, inputStream.Length);

            // set stream back to beginning
            HttpContext.Current.Request.InputStream.Position = 0;

            // get request string
            requestString = ASCIIEncoding.ASCII.GetString(inputStream);

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

        public static void FinalizeMonitorLogsData()
        {
            // dispose k-monitor
            (HttpContext.Current.Items[K_MON_KEY] as KMonitor).Dispose();
        }
    }
}
