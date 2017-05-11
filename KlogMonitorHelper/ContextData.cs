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
                switch (KMonitor.AppType)
                {
                    case KLogEnums.AppType.WCF:

                        if (OperationContext.Current != null)
                            SaveThreadParameters(OperationContext.Current);
                        break;

                    case KLogEnums.AppType.WS:
                    default:

                        if (HttpContext.Current != null)
                            SaveThreadParameters(HttpContext.Current);
                        break;
                }
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

                        if (threadParameters != null)
                            LoadThreadParameters(OperationContext.Current);
                        break;

                    case KLogEnums.AppType.WS:
                    default:

                        // set log configuration files
                        KLogger.Configure(log4netConfigFile, KLogEnums.AppType.WS);
                        KMonitor.Configure(log4netConfigFile, KLogEnums.AppType.WS);

                        if (threadParameters != null)
                            LoadThreadParameters(HttpContext.Current);

                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to load context data to a new thread", ex);
            }
        }

        private void SaveThreadParameters(HttpContext context)
        {
            if (context == null)
                context = new HttpContext(new HttpRequest("", "http://tempuri.org", ""), new HttpResponse(new StringWriter()));

            if (context.Items[KLogMonitor.Constants.REQUEST_ID_KEY] != null)
                threadParameters[KLogMonitor.Constants.REQUEST_ID_KEY] = context.Items[KLogMonitor.Constants.REQUEST_ID_KEY].ToString();

            if (context.Items[KLogMonitor.Constants.ACTION] != null)
                threadParameters[KLogMonitor.Constants.ACTION] = context.Items[KLogMonitor.Constants.ACTION].ToString();

            if (context.Items[KLogMonitor.Constants.CLIENT_TAG] != null)
                threadParameters[KLogMonitor.Constants.CLIENT_TAG] = context.Items[KLogMonitor.Constants.CLIENT_TAG].ToString();

            if (context.Items[KLogMonitor.Constants.GROUP_ID] != null)
                threadParameters[KLogMonitor.Constants.GROUP_ID] = context.Items[KLogMonitor.Constants.GROUP_ID].ToString();

            if (context.Items[KLogMonitor.Constants.HOST_IP] != null)
                threadParameters[KLogMonitor.Constants.HOST_IP] = context.Items[KLogMonitor.Constants.HOST_IP].ToString();

            if (context.Items[KLogMonitor.Constants.TOPIC] != null)
                threadParameters[KLogMonitor.Constants.TOPIC] = context.Items[KLogMonitor.Constants.TOPIC].ToString();

            if (context.Items[KLogMonitor.Constants.USER_ID] != null)
                threadParameters[KLogMonitor.Constants.USER_ID] = context.Items[KLogMonitor.Constants.USER_ID].ToString();
        }

        private void SaveThreadParameters(OperationContext context)
        {
            object temp = null;
            if (context.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.REQUEST_ID_KEY, out temp))
                threadParameters[KLogMonitor.Constants.REQUEST_ID_KEY] = temp.ToString();

            if (context.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.ACTION, out temp))
                threadParameters[KLogMonitor.Constants.ACTION] = temp.ToString();

            if (context.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.CLIENT_TAG, out temp))
                threadParameters[KLogMonitor.Constants.CLIENT_TAG] = temp.ToString();

            if (context.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.GROUP_ID, out temp))
                threadParameters[KLogMonitor.Constants.GROUP_ID] = temp.ToString();

            if (context.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.HOST_IP, out temp))
                threadParameters[KLogMonitor.Constants.HOST_IP] = temp.ToString();

            if (context.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.TOPIC, out temp))
                threadParameters[KLogMonitor.Constants.TOPIC] = temp.ToString();

            if (context.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.USER_ID, out temp))
                threadParameters[KLogMonitor.Constants.USER_ID] = temp.ToString();
        }

        private void LoadThreadParameters(HttpContext context)
        {
            if (context == null)
                context = new HttpContext(new HttpRequest("", "http://tempuri.org", ""), new HttpResponse(new StringWriter()));

            string temp = string.Empty;
            if (threadParameters.TryGetValue(KLogMonitor.Constants.REQUEST_ID_KEY, out temp))
                context.Items[KLogMonitor.Constants.REQUEST_ID_KEY] = temp;

            if (threadParameters.TryGetValue(KLogMonitor.Constants.ACTION, out temp))
                context.Items[KLogMonitor.Constants.ACTION] = temp;

            if (threadParameters.TryGetValue(KLogMonitor.Constants.CLIENT_TAG, out temp))
                context.Items[KLogMonitor.Constants.CLIENT_TAG] = temp;

            if (threadParameters.TryGetValue(KLogMonitor.Constants.GROUP_ID, out temp))
                context.Items[KLogMonitor.Constants.GROUP_ID] = temp;

            if (threadParameters.TryGetValue(KLogMonitor.Constants.HOST_IP, out temp))
                context.Items[KLogMonitor.Constants.HOST_IP] = temp;

            if (threadParameters.TryGetValue(KLogMonitor.Constants.TOPIC, out temp))
                context.Items[KLogMonitor.Constants.TOPIC] = temp;

            if (threadParameters.TryGetValue(KLogMonitor.Constants.USER_ID, out temp))
                context.Items[KLogMonitor.Constants.USER_ID] = temp;
        }

        private void LoadThreadParameters(OperationContext context)
        {
            string temp = string.Empty;
            if (threadParameters.TryGetValue(KLogMonitor.Constants.REQUEST_ID_KEY, out temp))
                context.IncomingMessageProperties[KLogMonitor.Constants.REQUEST_ID_KEY] = temp;

            if (threadParameters.TryGetValue(KLogMonitor.Constants.ACTION, out temp))
                context.IncomingMessageProperties[KLogMonitor.Constants.ACTION] = temp;

            if (threadParameters.TryGetValue(KLogMonitor.Constants.CLIENT_TAG, out temp))
                context.IncomingMessageProperties[KLogMonitor.Constants.CLIENT_TAG] = temp;

            if (threadParameters.TryGetValue(KLogMonitor.Constants.GROUP_ID, out temp))
                context.IncomingMessageProperties[KLogMonitor.Constants.GROUP_ID] = temp;

            if (threadParameters.TryGetValue(KLogMonitor.Constants.HOST_IP, out temp))
                context.IncomingMessageProperties[KLogMonitor.Constants.HOST_IP] = temp;

            if (threadParameters.TryGetValue(KLogMonitor.Constants.TOPIC, out temp))
                context.IncomingMessageProperties[KLogMonitor.Constants.TOPIC] = temp;

            if (threadParameters.TryGetValue(KLogMonitor.Constants.USER_ID, out temp))
                context.IncomingMessageProperties[KLogMonitor.Constants.USER_ID] = temp;
        }
    }
}
