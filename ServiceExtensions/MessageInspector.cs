using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Diagnostics.Eventing;
using KLogMonitor;
using System.ServiceModel;
using TVinciShared;
using System.Xml;
using System.ServiceModel.Web;
using System.Net;
using KlogMonitorHelper;
using System.Reflection;

namespace ServiceExtensions
{
    public class MessageInspector : IDispatchMessageInspector
    {
        //private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public object AfterReceiveRequest(ref Message request,
          System.ServiceModel.IClientChannel channel,
          System.ServiceModel.InstanceContext instanceContext)
        {
            // initialize monitor and logs parameters
            MonitorLogsHelper.InitMonitorLogsDataWCF(request);
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            // finalize monitor and logs
            MonitorLogsHelper.FinalizeMonitorLogsData(KLogEnums.AppType.WCF);

            // log response
            //string replyMessage = string.Empty;
            //if (reply != null)
            //    log.Debug("RESPONSE STRING:" + replyMessage.ToString());
        }
    }
}