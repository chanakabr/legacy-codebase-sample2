using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TVinciShared
{
    public static class WCFHelpers
    {
        public static ClientBase<TChannel> ConfigureServiceClient<TChannel>(this ClientBase<TChannel> serviceToConfigure) where TChannel : class
        {
            var addRequestIdToHeadersBehaviour = new ServiceExtensions.ClientEndpointBehavior();
            serviceToConfigure.Endpoint.EndpointBehaviors.Add(addRequestIdToHeadersBehaviour);
            // TODO: Configure other properties for every adapter form tcm as web.config is not here anymore
            return serviceToConfigure;
        }
    }
}
