using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Configuration;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Xml;
using KLogMonitor;
using System.Reflection;

namespace ServiceExtensions
{
    public class LoggerWCFInspector : IDispatchMessageInspector
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            log.Debug("in LoggerWCFInspector");

            string messageStr;

            getMessageStr(ref request, out messageStr);

            MessageState messageState = new MessageState(ref request);

            //BaseLog oldLog = createLoggerObject(messageState.ID, messageState.MethodName, eLogType.WcfRequest, messageState.CreationDate, ref messageStr);

            //log.Info(messageStr);

            return messageState;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            string messageStr;
            string serviceName = string.Empty;
            string methodName = string.Empty;
            string messageID = string.Empty;
            DateTime creationDate = DateTime.UtcNow;

            getMessageStr(ref reply, out messageStr);


            if (correlationState is MessageState)
            {
                MessageState msgState = correlationState as MessageState;
                methodName = msgState.MethodName;
                messageID = msgState.ID;
                creationDate = msgState.CreationDate;

            }

            //BaseLog log = createLoggerObject(messageID, methodName, eLogType.WcfResponse, creationDate, ref messageStr);

            //log.Info(messageStr, true);
        }

        //private BaseLog createLoggerObject(string threadID, string methodName, eLogType logType, DateTime utcTime, ref string message)
        //{
        //    return new BaseLog(utcTime)
        //    {
        //        TimeSpan = 0,
        //        Method = methodName,
        //        Id = threadID,
        //        Type = logType,
        //        Message = message
        //    };
        //}

        private void getMessageStr(ref Message msg, out string msgStr)
        {
            msgStr = string.Empty;
            if (msg != null)
            {
                using (MessageBuffer msgBuffer = msg.CreateBufferedCopy(int.MaxValue))
                {
                    using (var memStream = new MemoryStream())
                    {
                        try
                        {
                            XmlWriterSettings settings = new XmlWriterSettings();
                            settings.Encoding = System.Text.Encoding.UTF8;
                            XmlWriter writer = XmlWriter.Create(memStream, settings);

                            //Create a copy of the message
                            msg = msgBuffer.CreateMessage();
                            //Serialize the message to the XmlWriter 
                            msg.WriteMessage(writer);

                            //Flush the contents of the writer so that the stream gets updated
                            writer.Flush();
                            memStream.Flush();


                            byte[] retval = memStream.ToArray();
                            byte[] asciiBytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, retval);
                            msgStr = Encoding.ASCII.GetString(asciiBytes);
                        }
                        catch (Exception ex)
                        {
                            msgStr = string.Format("ERROR - Exception thrown when reading Logger WCF XML stream. Exception: {0}", ex.Message);

                        }
                    }

                    //must copy message as message can only be read onces. If we do not copy this buffered message the service will fail
                    msg = msgBuffer.CreateMessage();
                }
            }
        }

        private class MessageState
        {
            private const string sIDPattern = @"urn:uuid:";
            private const string sMethodPattern = @"urn:Iservice/";

            public string ID { get; set; }
            public string MethodName { get; set; }
            public string ServiceName { get; set; }
            public DateTime CreationDate { get; set; }

            public MessageState(ref Message message)
            {
                Init(ref message);
            }

            public void Init(ref Message message)
            {
                CreationDate = DateTime.UtcNow;
                ID = message.Headers.MessageId.ToString().Replace(sIDPattern, string.Empty);
                MethodName = message.Headers.Action.Replace(sMethodPattern, string.Empty);
            }
        }
    }

    public class LoggerMessageTracing : Attribute, IServiceBehavior
    {
        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription desc, ServiceHostBase host)
        {
            foreach (ChannelDispatcher cDispatcher in host.ChannelDispatchers)
                foreach (EndpointDispatcher eDispatcher in cDispatcher.Endpoints)
                    eDispatcher.DispatchRuntime.MessageInspectors.Add(new LoggerWCFInspector());
        }

        #region IServiceBehavior Members
        public void Validate(ServiceDescription desc, ServiceHostBase host)
        {
            //foreach (ServiceEndpoint se in desc.Endpoints)
            //    if (se.Binding.Name.Equals("BasicHttpBinding")) 
            //        throw new FaultException("BasicHttpBinding is not allowed");
        }
        #endregion

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {

        }
    }

    public class LoggerMessageTracingElement : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof(LoggerMessageTracingElement); }
        }
        protected override object CreateBehavior()
        {
            return new LoggerMessageTracingElement();
        }
    }
}
