#if NETFRAMEWORK
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using KLogMonitor;
using KlogMonitorHelper;
using System.Reflection;

namespace ServiceExtensions
{
    public class MessageInspector : IDispatchMessageInspector
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
            //if (reply != null)
            //    log.Debug("RESPONSE STRING:" + reply.ToString());
        }
    }
}
#endif