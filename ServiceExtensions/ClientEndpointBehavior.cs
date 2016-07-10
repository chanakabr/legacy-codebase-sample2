using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace ServiceExtensions
{
    public class ClientEndpointBehavior : BehaviorExtensionElement, IEndpointBehavior
    {
        public override Type BehaviorType
        {
            get { return typeof(ClientEndpointBehavior); }
        }

        protected override object CreateBehavior()
        {
            // Create the  endpoint behavior that will insert the message
            // inspector into the client runtime
            return new ClientEndpointBehavior();
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            // No implementation necessary
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new ClientMessageInspector());
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            // No implementation necessary
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            // No implementation necessary
        }
    }
}
