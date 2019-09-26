using ConfigurationManager;
using ConfigurationManager.Types;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TVinciShared
{
    public static class WCFHelpers
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static ClientBase<TChannel> ConfigureServiceClient<TChannel>(this ClientBase<TChannel> serviceToConfigure) where TChannel : class
        {
            _Logger.Debug($"Configuring service client:[{serviceToConfigure.GetType().FullName}]");
            var addRequestIdToHeadersBehaviour = new ServiceExtensions.ClientEndpointBehavior();
            serviceToConfigure.Endpoint.EndpointBehaviors.Add(addRequestIdToHeadersBehaviour);

            // TODO: Configure other properties for every adapter form tcm as web.config is not here anymore
            var adapterNamespace =  serviceToConfigure.Endpoint.Contract.ContractType.FullName;
            AdapterConfiguration adapterConfiguration = GetCurrentConfiguration(adapterNamespace);
            SetConfiguration(serviceToConfigure, adapterConfiguration);

            return serviceToConfigure;
        }
        

        private static void SetConfiguration<TChannel>(ClientBase<TChannel> serviceToConfigure, AdapterConfiguration adapterConfiguration) where TChannel : class
        {
            HttpBindingBase bindingBase = serviceToConfigure.Endpoint.Binding as HttpBindingBase;
            bindingBase.MaxReceivedMessageSize = adapterConfiguration.MaxReceivedMessageSize.Value;

            
            if (serviceToConfigure.Endpoint.Address.Uri.Scheme.ToLower().Equals("https"))
            {
                
                var securityMode = serviceToConfigure.Endpoint.Binding as BasicHttpBinding;
                securityMode.Security.Mode = BasicHttpSecurityMode.Transport;
                securityMode.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                //securityMode.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.Basic;
                
                //The username is not provided. Specify username in ClientCredentials
            }


            serviceToConfigure.Endpoint.Binding.SendTimeout = TimeSpan.FromSeconds(adapterConfiguration.SendTimeout.Value);
            serviceToConfigure.Endpoint.Binding.OpenTimeout = TimeSpan.FromSeconds(adapterConfiguration.OpenTimeout.Value);
            serviceToConfigure.Endpoint.Binding.CloseTimeout = TimeSpan.FromSeconds(adapterConfiguration.CloseTimeout.Value);
            serviceToConfigure.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromSeconds(adapterConfiguration.ReceiveTimeout.Value);
        }

        private static AdapterConfiguration GetCurrentConfiguration(string adapterNamespace)
        {
            adapterNamespace = adapterNamespace.Replace('.','_').ToLower();
            var currentConfiguration = ApplicationConfiguration.AdaptersConfiguration.configurationDictionary["default"];
            AdapterConfiguration adapterConfiguration;
            if (ApplicationConfiguration.AdaptersConfiguration.configurationDictionary.TryGetValue(adapterNamespace, out adapterConfiguration))
            {
                _Logger.Debug($"set specific configuration for Adaper:  {adapterNamespace}");
                currentConfiguration.CloseTimeout = adapterConfiguration.CloseTimeout ?? currentConfiguration.CloseTimeout;
                currentConfiguration.MaxReceivedMessageSize = adapterConfiguration.MaxReceivedMessageSize ?? currentConfiguration.MaxReceivedMessageSize;
                currentConfiguration.OpenTimeout = adapterConfiguration.OpenTimeout ?? currentConfiguration.OpenTimeout;
                currentConfiguration.ReceiveTimeout = adapterConfiguration.ReceiveTimeout ?? currentConfiguration.ReceiveTimeout;
                currentConfiguration.SendTimeout = adapterConfiguration.SendTimeout ?? currentConfiguration.SendTimeout;
            }
            return currentConfiguration;
        }
    }
}
