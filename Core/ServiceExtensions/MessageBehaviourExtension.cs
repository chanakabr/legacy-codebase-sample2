#if NETFRAMEWORK
using System;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
namespace ServiceExtensions
{
    public class MessageBehaviourExtension : BehaviorExtensionElement, IServiceBehavior
    {

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            //Logger.WriteLogEntry("Inside the ApplyClientBehavior");
        }

        public override Type BehaviorType
        {
            get { return typeof(MessageBehaviourExtension); }
        }

        protected override object CreateBehavior()
        {
            return this;
        }

        public void AddBindingParameters(ServiceDescription serviceDescription,
         System.ServiceModel.ServiceHostBase serviceHostBase,
         System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints,
         BindingParameterCollection bindingParameters)
        {
            //Logger.WriteLogEntry("Inside the AddBindingParameters");
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription,
         System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            //Logger.WriteLogEntry("Inside Apply Dispatch Behavior");
            for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher channelDispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                if (channelDispatcher != null)
                {
                    foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                    {
                        MessageInspector inspector = new MessageInspector();
                        endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
                    }
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription,
          System.ServiceModel.ServiceHostBase serviceHostBase)
        {

        }
    }
}
#endif