namespace ConfigurationManager.Types
{
    public class AdapterConfiguration
    {
        public int? SendTimeout { get; set; }
        public int? OpenTimeout { get; set; }
        public int? CloseTimeout { get; set; }
        public int? ReceiveTimeout { get; set; }
        public int? MaxReceivedMessageSize { get; set; }
    }
}