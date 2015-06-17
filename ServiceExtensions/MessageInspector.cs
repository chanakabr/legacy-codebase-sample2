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
namespace ServiceExtensions
{
    public class MessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref Message request,
          System.ServiceModel.IClientChannel channel,
          System.ServiceModel.InstanceContext instanceContext)
        {
            OperationContext.Current.IncomingMessageProperties.Add(Constants.GROUP_ID, "bla");

            // Logger.WriteLogEntry("Inside the AfterRecieveRequest");
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {

            var t = OperationContext.Current.IncomingMessageProperties[Constants.GROUP_ID];


            //   Logger.WriteLogEntry("Inside Before Send Reply");
        }
    }
}