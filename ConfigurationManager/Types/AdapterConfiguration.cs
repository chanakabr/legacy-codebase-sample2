using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager.Types
{
    public class AdapterConfiguration 
    {
        public BaseValue<int?> OpenTimeout = new BaseValue<int?>("openTimeout", 10);
        public BaseValue<int?> ReceiveTimeout = new BaseValue<int?>("receiveTimeout", 10);
        public BaseValue<int?> SendTimeout = new BaseValue<int?>("sendTimeout", 10);
        public BaseValue<int?> CloseTimeout = new BaseValue<int?>("closeTimeout", 60);
        public BaseValue<int?> MaxReceivedMessageSize = new BaseValue<int?>("maxReceivedMessageSize", 2147483647);

        public AdapterConfiguration DeepCopy()
        {
            AdapterConfiguration res = new AdapterConfiguration()
            {
                CloseTimeout = CloseTimeout.DeepCopy(),
                MaxReceivedMessageSize = MaxReceivedMessageSize.DeepCopy(),
                OpenTimeout = OpenTimeout.DeepCopy(),
                ReceiveTimeout = ReceiveTimeout.DeepCopy(),
                SendTimeout = SendTimeout.DeepCopy()
            };
            return res;
        }
    }
}