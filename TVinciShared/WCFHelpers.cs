using ConfigurationManager;
using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using ConfigurationManager.Types;
using KLogMonitor;
using System;
using System.Reflection;
using System.ServiceModel;

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

            var adapterNamespace = serviceToConfigure.Endpoint.Contract.ContractType.FullName;
            AdapterConfiguration adapterConfiguration = GetCurrentConfiguration(adapterNamespace);
            SetConfiguration(serviceToConfigure, adapterConfiguration);

            return serviceToConfigure;
        }

        private static void SetConfiguration<TChannel>(ClientBase<TChannel> serviceToConfigure, AdapterConfiguration adapterConfiguration) where TChannel : class
        {
            var bindingBase = serviceToConfigure.Endpoint.Binding as HttpBindingBase;

            bindingBase.MaxReceivedMessageSize = adapterConfiguration.MaxReceivedMessageSize.Value.GetValueOrDefault();
            bindingBase.SendTimeout = TimeSpan.FromSeconds(adapterConfiguration.SendTimeout.Value.GetValueOrDefault());
            bindingBase.OpenTimeout = TimeSpan.FromSeconds(adapterConfiguration.OpenTimeout.Value.GetValueOrDefault());
            bindingBase.CloseTimeout = TimeSpan.FromSeconds(adapterConfiguration.CloseTimeout.Value.GetValueOrDefault());
            bindingBase.ReceiveTimeout = TimeSpan.FromSeconds(adapterConfiguration.ReceiveTimeout.Value.GetValueOrDefault());
            bindingBase.MaxBufferSize = (int)bindingBase.MaxReceivedMessageSize;
            
            if (serviceToConfigure.Endpoint.Address.Uri.Scheme.ToLower().Equals("https"))
            {
                var securityMode = serviceToConfigure.Endpoint.Binding as BasicHttpBinding;
                securityMode.Security.Mode = BasicHttpSecurityMode.Transport;
                securityMode.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            }
        }

        private static AdapterConfiguration GetCurrentConfiguration(string adapterNamespace)
        {
            adapterNamespace = adapterNamespace.Replace('.','_').ToLower();
            AdapterConfiguration defaultConfiguration = ApplicationConfiguration.Current.AdaptersConfiguration.ConfigurationDictionary.Value[TcmObjectKeys.DefaultConfigurationKey];
            if (ApplicationConfiguration.Current.AdaptersConfiguration.ConfigurationDictionary.Value.TryGetValue(adapterNamespace, out var specificConfiguration))
            {
                _Logger.Debug($"set specific configuration for Adapter:  {adapterNamespace}");
                defaultConfiguration.CloseTimeout = specificConfiguration.CloseTimeout ?? defaultConfiguration.CloseTimeout;
                defaultConfiguration.MaxReceivedMessageSize = specificConfiguration.MaxReceivedMessageSize ?? defaultConfiguration.MaxReceivedMessageSize;
                defaultConfiguration.OpenTimeout = specificConfiguration.OpenTimeout ?? defaultConfiguration.OpenTimeout;
                defaultConfiguration.ReceiveTimeout = specificConfiguration.ReceiveTimeout ?? defaultConfiguration.ReceiveTimeout;
                defaultConfiguration.SendTimeout = specificConfiguration.SendTimeout ?? defaultConfiguration.SendTimeout;
                defaultConfiguration.HttpClientCredentialType = specificConfiguration.HttpClientCredentialType ?? defaultConfiguration.HttpClientCredentialType;
            }

            return defaultConfiguration;
        }
    }
}
