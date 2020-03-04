using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager.Types
{
    public class AdapterConfiguration 
    {
        public BaseValue<int?> OpenTimeout = new BaseValue<int?>("openTimeout", 10);
        public BaseValue<int?> ReceiveTimeout = new BaseValue<int?>("receiveTimeout", 10);
        public BaseValue<int?> SendTimeout = new BaseValue<int?>("sendTimeout", 10);
        public BaseValue<int?> CloseTimeout = new BaseValue<int?>("closeTimeout", 60);
        public BaseValue<long?> MaxReceivedMessageSize = new BaseValue<long?>("maxReceivedMessageSize", 2147483647);
        public BaseValue<System.ServiceModel.HttpClientCredentialType?> HttpClientCredentialType = new BaseValue<System.ServiceModel.HttpClientCredentialType?>("httpClientCredentialType", System.ServiceModel.HttpClientCredentialType.None);

        internal static  AdapterConfiguration Copy(AdapterConfiguration copyFrom)
        {
            AdapterConfiguration res = new AdapterConfiguration()
            {
                CloseTimeout = copyFrom.CloseTimeout,
                MaxReceivedMessageSize = copyFrom.MaxReceivedMessageSize,
                OpenTimeout = copyFrom.OpenTimeout,
                ReceiveTimeout = copyFrom.ReceiveTimeout,
                SendTimeout = copyFrom.SendTimeout,
                HttpClientCredentialType = copyFrom.HttpClientCredentialType
            };

            return res;
        }
    }
}