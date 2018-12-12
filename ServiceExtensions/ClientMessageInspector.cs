using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using KLogMonitor;

namespace ServiceExtensions
{
    public class ClientMessageInspector : IClientMessageInspector
    {
        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            // Implement this method to inspect/modify messages after a message
            // is received but prior to passing it back to the client 
            //Console.WriteLine("AfterReceiveReply called");
        }

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
        {
            // Implement this method to inspect/modify messages before they 
            // are sent to the service

            // add request ID to message header
            if (request.Headers != null &&
                request.Headers.FindHeader(KLogMonitor.Constants.REQUEST_ID_KEY.ToString(), string.Empty) == -1)
            {
                if (KLogMonitor.KLogger.AppType == KLogEnums.AppType.WCF)
                {
                    if (OperationContext.Current != null)
                    {
                        object res = null;
                        if (OperationContext.Current.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.REQUEST_ID_KEY, out res))
                            request.Headers.Add(MessageHeader.CreateHeader(KLogMonitor.Constants.REQUEST_ID_KEY, string.Empty, res.ToString()));

                        if (OperationContext.Current.IncomingMessageProperties.TryGetValue(KLogMonitor.Constants.KS, out res))
                            request.Headers.Add(MessageHeader.CreateHeader(KLogMonitor.Constants.KS, string.Empty, res.ToString()));
                    }
                }
                else
                {
                    if (HttpContext.Current != null)
                    {
                        if (HttpContext.Current.Items[KLogMonitor.Constants.REQUEST_ID_KEY] != null)
                            request.Headers.Add(MessageHeader.CreateHeader(KLogMonitor.Constants.REQUEST_ID_KEY, string.Empty, HttpContext.Current.Items[KLogMonitor.Constants.REQUEST_ID_KEY].ToString()));

                        if (HttpContext.Current.Items[KLogMonitor.Constants.KS] != null)
                            request.Headers.Add(MessageHeader.CreateHeader(KLogMonitor.Constants.KS, string.Empty, HttpContext.Current.Items[KLogMonitor.Constants.KS].ToString()));
                    }
                }
            }

            return null;
        }
    }
}
