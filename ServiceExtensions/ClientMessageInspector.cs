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
            if (request.Headers.FindHeader(KLogMonitor.Constants.REQUEST_ID_KEY.ToString(), string.Empty) == -1 &&
                HttpContext.Current != null &&
                HttpContext.Current.Items[Constants.REQUEST_ID_KEY] != null)
            {
                request.Headers.Add(MessageHeader.CreateHeader(KLogMonitor.Constants.REQUEST_ID_KEY.ToString(), string.Empty, HttpContext.Current.Items[Constants.REQUEST_ID_KEY]));
            }

            return null;
        }
    }
}
