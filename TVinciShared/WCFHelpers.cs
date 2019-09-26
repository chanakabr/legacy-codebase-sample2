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

            //todo: check null if default not exist
            var adapterNamespace =  serviceToConfigure.Endpoint.Contract.ContractType.FullName;
            AdapterConfiguration adapterConfiguration = GetCurrentConfiguration(adapterNamespace);
            SetConfiguration(serviceToConfigure, adapterConfiguration);

            return serviceToConfigure;
        }
        

        private static void SetConfiguration<TChannel>(ClientBase<TChannel> serviceToConfigure, AdapterConfiguration adapterConfiguration) where TChannel : class
        {
            var bindingBase = serviceToConfigure.Endpoint.Binding as HttpBindingBase;
            bindingBase.MaxReceivedMessageSize = adapterConfiguration.MaxReceivedMessageSize.Value;

            if (serviceToConfigure.Endpoint.Address.Uri.Scheme.ToLower().Equals("https"))
            {
                var securityMode = serviceToConfigure.Endpoint.Binding as BasicHttpBinding;
                securityMode.Security.Mode = BasicHttpSecurityMode.Transport;
                securityMode.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            }

            serviceToConfigure.Endpoint.Binding.SendTimeout = TimeSpan.FromSeconds(adapterConfiguration.SendTimeout.Value);
            serviceToConfigure.Endpoint.Binding.OpenTimeout = TimeSpan.FromSeconds(adapterConfiguration.OpenTimeout.Value);
            serviceToConfigure.Endpoint.Binding.CloseTimeout = TimeSpan.FromSeconds(adapterConfiguration.CloseTimeout.Value);
            serviceToConfigure.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromSeconds(adapterConfiguration.ReceiveTimeout.Value);
        }

        private static AdapterConfiguration GetCurrentConfiguration(string adapterNamespace)
        {
            adapterNamespace = adapterNamespace.Replace('.','_').ToLower();
           //todo: check null if default not exist
            var defaultConfiguration = ApplicationConfiguration.AdaptersConfiguration.configurationDictionary["default"];
            if (ApplicationConfiguration.AdaptersConfiguration.configurationDictionary.TryGetValue(adapterNamespace, out var specificConfiguration))
            {
                _Logger.Debug($"set specific configuration for Adapter:  {adapterNamespace}");
                defaultConfiguration.CloseTimeout = specificConfiguration.CloseTimeout ?? defaultConfiguration.CloseTimeout;
                defaultConfiguration.MaxReceivedMessageSize = specificConfiguration.MaxReceivedMessageSize ?? defaultConfiguration.MaxReceivedMessageSize;
                defaultConfiguration.OpenTimeout = specificConfiguration.OpenTimeout ?? defaultConfiguration.OpenTimeout;
                defaultConfiguration.ReceiveTimeout = specificConfiguration.ReceiveTimeout ?? defaultConfiguration.ReceiveTimeout;
                defaultConfiguration.SendTimeout = specificConfiguration.SendTimeout ?? defaultConfiguration.SendTimeout;
            }

            return defaultConfiguration;
        }
    }
}
