namespace ConfigurationManager.Types
{
    public class AdapterConfiguration
    {
        internal static AdapterConfiguration DefaultConfig = new AdapterConfiguration()
        {
            OpenTimeout = 10,
            ReceiveTimeout = 10,
            SendTimeout = 10,
            CloseTimeout = 60,
            MaxReceivedMessageSize = 5242880 //5 MB in bytes
        };

        public int? SendTimeout { get; set; }
        public int? OpenTimeout { get; set; }
        public int? CloseTimeout { get; set; }
        public int? ReceiveTimeout { get; set; }
        public int? MaxReceivedMessageSize { get; set; }
    }
}