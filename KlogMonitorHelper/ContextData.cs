using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Web;
using KLogMonitor;

namespace KlogMonitorHelper
{
    public class ContextData
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private string defaultLog4netConfigFile = "log4net.config";
        private Dictionary<string, string> threadParameters = new Dictionary<string, string>();

        public ContextData()
        {
            try
            {
                SaveThreadParameters();
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to copy context data to a new thread", ex);
            }
        }

        public void Load(string log4netConfigFile = null)
        {
            if (log4netConfigFile == null)
                log4netConfigFile = defaultLog4netConfigFile;

            try
            {
                switch (KMonitor.AppType)
                {
                    case KLogEnums.AppType.WCF:

                        // set log configuration files
                        KLogger.Configure(log4netConfigFile, KLogEnums.AppType.WCF);
                        KMonitor.Configure(log4netConfigFile, KLogEnums.AppType.WCF);
                        break;

                    case KLogEnums.AppType.WS:
                    default:

                        // set log configuration files
                        KLogger.Configure(log4netConfigFile, KLogEnums.AppType.WS);
                        KMonitor.Configure(log4netConfigFile, KLogEnums.AppType.WS);
                        break;
                }

                if (threadParameters != null)
                    LoadThreadParameters();
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to load context data to a new thread", ex);
            }
        }

        private void SaveThreadParameters()
        {
            switch (KMonitor.AppType)
            {
                case KLogEnums.AppType.WCF:

                    object temp = null;
                    if (OperationContext.Current.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.REQUEST_ID_KEY, out temp))
                        threadParameters[KLogMonitor.Constants.REQUEST_ID_KEY] = temp.ToString();

                    if (OperationContext.Current.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.ACTION, out temp))
                        threadParameters[KLogMonitor.Constants.ACTION] = temp.ToString();

                    if (OperationContext.Current.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.CLIENT_TAG, out temp))
                        threadParameters[KLogMonitor.Constants.CLIENT_TAG] = temp.ToString();

                    if (OperationContext.Current.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.GROUP_ID, out temp))
                        threadParameters[KLogMonitor.Constants.GROUP_ID] = temp.ToString();

                    if (OperationContext.Current.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.HOST_IP, out temp))
                        threadParameters[KLogMonitor.Constants.HOST_IP] = temp.ToString();

                    if (OperationContext.Current.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.TOPIC, out temp))
                        threadParameters[KLogMonitor.Constants.TOPIC] = temp.ToString();

                    if (OperationContext.Current.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.USER_ID, out temp))
                        threadParameters[KLogMonitor.Constants.USER_ID] = temp.ToString();
                    break;

                case KLogEnums.AppType.WS:
                default:

                    if (HttpContext.Current == null)
                        HttpContext.Current = new HttpContext(new HttpRequest("", "http://tempuri.org", ""), new HttpResponse(new StringWriter()));

                    if (HttpContext.Current.Items[KLogMonitor.Constants.REQUEST_ID_KEY] != null)
                        threadParameters[KLogMonitor.Constants.REQUEST_ID_KEY] = HttpContext.Current.Items[KLogMonitor.Constants.REQUEST_ID_KEY].ToString();

                    if (HttpContext.Current.Items[KLogMonitor.Constants.ACTION] != null)
                        threadParameters[KLogMonitor.Constants.ACTION] = HttpContext.Current.Items[KLogMonitor.Constants.ACTION].ToString();

                    if (HttpContext.Current.Items[KLogMonitor.Constants.CLIENT_TAG] != null)
                        threadParameters[KLogMonitor.Constants.CLIENT_TAG] = HttpContext.Current.Items[KLogMonitor.Constants.CLIENT_TAG].ToString();

                    if (HttpContext.Current.Items[KLogMonitor.Constants.GROUP_ID] != null)
                        threadParameters[KLogMonitor.Constants.GROUP_ID] = HttpContext.Current.Items[KLogMonitor.Constants.GROUP_ID].ToString();

                    if (HttpContext.Current.Items[KLogMonitor.Constants.HOST_IP] != null)
                        threadParameters[KLogMonitor.Constants.HOST_IP] = HttpContext.Current.Items[KLogMonitor.Constants.HOST_IP].ToString();

                    if (HttpContext.Current.Items[KLogMonitor.Constants.TOPIC] != null)
                        threadParameters[KLogMonitor.Constants.TOPIC] = HttpContext.Current.Items[KLogMonitor.Constants.TOPIC].ToString();

                    if (HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] != null)
                        threadParameters[KLogMonitor.Constants.USER_ID] = HttpContext.Current.Items[KLogMonitor.Constants.USER_ID].ToString();
                    break;
            }
        }

        private void LoadThreadParameters()
        {
            string temp = string.Empty;
            switch (KMonitor.AppType)
            {
                case KLogEnums.AppType.WCF:

                    // TODO: no solution yet for OperationContext.Current == null) 
                    if (OperationContext.Current == null)
                        return;

                    if (threadParameters.TryGetValue(KLogMonitor.Constants.REQUEST_ID_KEY, out temp))
                        OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.REQUEST_ID_KEY] = temp;

                    if (threadParameters.TryGetValue(KLogMonitor.Constants.ACTION, out temp))
                        OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.ACTION] = temp;

                    if (threadParameters.TryGetValue(KLogMonitor.Constants.CLIENT_TAG, out temp))
                        OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.CLIENT_TAG] = temp;

                    if (threadParameters.TryGetValue(KLogMonitor.Constants.GROUP_ID, out temp))
                        OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.GROUP_ID] = temp;

                    if (threadParameters.TryGetValue(KLogMonitor.Constants.HOST_IP, out temp))
                        OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.HOST_IP] = temp;

                    if (threadParameters.TryGetValue(KLogMonitor.Constants.TOPIC, out temp))
                        OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.TOPIC] = temp;

                    if (threadParameters.TryGetValue(KLogMonitor.Constants.USER_ID, out temp))
                        OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.USER_ID] = temp;

                    break;

                case KLogEnums.AppType.WS:
                default:

                    if (HttpContext.Current == null)
                        HttpContext.Current = new HttpContext(new HttpRequest("", "http://tempuri.org", ""), new HttpResponse(new StringWriter()));

                    if (threadParameters.TryGetValue(KLogMonitor.Constants.REQUEST_ID_KEY, out temp))
                        HttpContext.Current.Items[KLogMonitor.Constants.REQUEST_ID_KEY] = temp;

                    if (threadParameters.TryGetValue(KLogMonitor.Constants.ACTION, out temp))
                        HttpContext.Current.Items[KLogMonitor.Constants.ACTION] = temp;

                    if (threadParameters.TryGetValue(KLogMonitor.Constants.CLIENT_TAG, out temp))
                        HttpContext.Current.Items[KLogMonitor.Constants.CLIENT_TAG] = temp;

                    if (threadParameters.TryGetValue(KLogMonitor.Constants.GROUP_ID, out temp))
                        HttpContext.Current.Items[KLogMonitor.Constants.GROUP_ID] = temp;

                    if (threadParameters.TryGetValue(KLogMonitor.Constants.HOST_IP, out temp))
                        HttpContext.Current.Items[KLogMonitor.Constants.HOST_IP] = temp;

                    if (threadParameters.TryGetValue(KLogMonitor.Constants.TOPIC, out temp))
                        HttpContext.Current.Items[KLogMonitor.Constants.TOPIC] = temp;

                    if (threadParameters.TryGetValue(KLogMonitor.Constants.USER_ID, out temp))
                        HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = temp;

                    break;
            }
        }
    }

    [ServiceContract()]
    interface Itest
    {
        [OperationContract()]
        double Add(double A, double B);
    }
}
