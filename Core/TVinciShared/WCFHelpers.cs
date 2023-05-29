using Phx.Lib.Appconfig;
using Phx.Lib.Appconfig.Types;
using Phx.Lib.Log;
using System;
using System.Reflection;
using System.ServiceModel;
using Phx.Lib.Appconfig.Settings.Base;

namespace TVinciShared
{
    public static class WCFHelpers
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static ClientBase<TChannel> ConfigureServiceClient<TChannel>(this ClientBase<TChannel> serviceToConfigure, AdaptersClientConfiguration.AdapterClientConfiguration adapterConfiguration = null) where TChannel : class
        {
            _Logger.Debug($"Configuring service client:[{serviceToConfigure.GetType().FullName}]");
            var addRequestIdToHeadersBehaviour = new ServiceExtensions.ClientEndpointBehavior();
            serviceToConfigure.Endpoint.EndpointBehaviors.Add(addRequestIdToHeadersBehaviour);

            SetConfiguration(serviceToConfigure, adapterConfiguration ?? ApplicationConfiguration.Current.AdaptersClientConfiguration.DefaultAdapter);

            return serviceToConfigure;
        }

        private static void SetConfiguration<TChannel>(ClientBase<TChannel> serviceToConfigure, AdaptersClientConfiguration.AdapterClientConfiguration adapterConfiguration) where TChannel : class
        {
            var bindingBase = serviceToConfigure.Endpoint.Binding as HttpBindingBase;

            bindingBase.MaxReceivedMessageSize = adapterConfiguration.MaxReceivedMessageSize.Value;
            bindingBase.SendTimeout = TimeSpan.FromMilliseconds(adapterConfiguration.SendTimeoutMs.Value);
            bindingBase.OpenTimeout = TimeSpan.FromMilliseconds(adapterConfiguration.OpenTimeoutMs.Value);
            bindingBase.CloseTimeout = TimeSpan.FromMilliseconds(adapterConfiguration.CloseTimeoutMs.Value);
            bindingBase.ReceiveTimeout = TimeSpan.FromMilliseconds(adapterConfiguration.ReceiveTimeoutMs.Value);
            bindingBase.MaxBufferSize = (int)bindingBase.MaxReceivedMessageSize;
            
            if (serviceToConfigure.Endpoint.Address.Uri.Scheme.ToLower().Equals("https"))
            {
                var securityMode = serviceToConfigure.Endpoint.Binding as BasicHttpBinding;
                securityMode.Security.Mode = BasicHttpSecurityMode.Transport;
                securityMode.Security.Transport.ClientCredentialType = adapterConfiguration.HttpClientCredentialType != null ? adapterConfiguration.HttpClientCredentialType.Value : HttpClientCredentialType.None;
            }
        }
    }
}
