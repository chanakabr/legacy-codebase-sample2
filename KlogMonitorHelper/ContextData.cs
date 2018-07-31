using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
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
        private ContextDataObject contextData = new ContextDataObject();

        public ContextData()
        {
            try
            {
                contextData.data = new Dictionary<string, string>();

                switch (KMonitor.AppType)
                {
                    case KLogEnums.AppType.WCF:

                        if (OperationContext.Current != null)
                        {

                            object temp;
                            if (OperationContext.Current.IncomingMessageProperties.TryGetValue(Constants.CLIENT_TAG, out temp))
                                contextData.data[Constants.CLIENT_TAG] = temp.ToString();

                            if (OperationContext.Current.IncomingMessageProperties.TryGetValue(Constants.HOST_IP, out temp))
                                contextData.data[Constants.HOST_IP] = temp.ToString();

                            if (OperationContext.Current.IncomingMessageProperties.TryGetValue(Constants.REQUEST_ID_KEY, out temp))
                                contextData.data[Constants.REQUEST_ID_KEY] = temp.ToString();

                            if (OperationContext.Current.IncomingMessageProperties.TryGetValue(Constants.GROUP_ID, out temp))
                                contextData.data[Constants.GROUP_ID] = temp.ToString();

                            if (OperationContext.Current.IncomingMessageProperties.TryGetValue(Constants.ACTION, out temp))
                                contextData.data[Constants.ACTION] = temp.ToString();

                            if (OperationContext.Current.IncomingMessageProperties.TryGetValue(Constants.USER_ID, out temp))
                                contextData.data[Constants.USER_ID] = temp.ToString();

                            if (OperationContext.Current.IncomingMessageProperties.TryGetValue(Constants.TOPIC, out temp))
                                contextData.data[Constants.TOPIC] = temp.ToString();

                            if (OperationContext.Current.IncomingMessageProperties.TryGetValue(Constants.KS, out temp))
                                contextData.data[Constants.KS] = temp.ToString();
                        }
                        break;

                    case KLogEnums.AppType.WS:
                    default:

                        if (HttpContext.Current != null)
                        {
                            if (HttpContext.Current.Items[Constants.CLIENT_TAG] != null)
                                contextData.data[Constants.CLIENT_TAG] = HttpContext.Current.Items[Constants.CLIENT_TAG].ToString();

                            if (HttpContext.Current.Items[Constants.HOST_IP] != null)
                                contextData.data[Constants.HOST_IP] = HttpContext.Current.Items[Constants.HOST_IP].ToString();

                            if (HttpContext.Current.Items[Constants.REQUEST_ID_KEY] != null)
                                contextData.data[Constants.REQUEST_ID_KEY] = HttpContext.Current.Items[Constants.REQUEST_ID_KEY].ToString();

                            if (HttpContext.Current.Items[Constants.GROUP_ID] != null)
                                contextData.data[Constants.GROUP_ID] = HttpContext.Current.Items[Constants.GROUP_ID].ToString();

                            if (HttpContext.Current.Items[Constants.ACTION] != null)
                                contextData.data[Constants.ACTION] = HttpContext.Current.Items[Constants.ACTION].ToString();

                            if (HttpContext.Current.Items[Constants.USER_ID] != null)
                                contextData.data[Constants.USER_ID] = HttpContext.Current.Items[Constants.USER_ID].ToString();

                            if (HttpContext.Current.Items[Constants.TOPIC] != null)
                                contextData.data[Constants.TOPIC] = HttpContext.Current.Items[Constants.TOPIC].ToString();
                        }
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
                        break;

                    case KLogEnums.AppType.WS:
                    default:

                        // set log configuration files
                        KLogger.Configure(log4netConfigFile, KLogEnums.AppType.WS);
                        KMonitor.Configure(log4netConfigFile, KLogEnums.AppType.WS);
                        break;
                }

                if (contextData != null && contextData.data != null)
                    CallContext.SetData(KLogMonitor.Constants.MULTI_THREAD_DATA_KEY, contextData);
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to load context data to a new thread", ex);
            }
        }
    }
}
