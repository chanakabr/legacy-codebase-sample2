namespace ConfigurationManager
{
    public class EutelsatSettings : ConfigurationValue
    {
        public StringConfigurationValue Eutelsat_CheckTvod_ws;
        public StringConfigurationValue Eutelsat_Transaction_ws;
        public StringConfigurationValue Eutelsat_Subscription_ws;
        public StringConfigurationValue Eutelsat_3SS_WS_Username;
        public StringConfigurationValue Eutelsat_3SS_WS_Password;
        public StringConfigurationValue Eutelsat_ProductBase;
        public NumericConfigurationValue RightMargin;
        public NumericConfigurationValue LeftMargin;
        public NumericConfigurationValue TimeMultFactor;
        public BooleanConfigurationValue Skip3SSCheck;
        public StringConfigurationValue Transaction_Device_Filter;

        public EutelsatSettings(string key) : base(key)
        {
            Eutelsat_CheckTvod_ws = new StringConfigurationValue("Eutelsat_CheckTvod_ws", this)
            {
                OriginalKey = "Eutelsat_CheckTvod_ws"
            };
            Eutelsat_Transaction_ws = new StringConfigurationValue("Eutelsat_Transaction_ws", this)
            {
                OriginalKey = "Eutelsat_Transaction_ws"
            };
            Eutelsat_Subscription_ws = new StringConfigurationValue("Eutelsat_Subscription_ws", this)
            {
                OriginalKey = "Eutelsat_Subscription_ws"
            };
            Eutelsat_3SS_WS_Username = new StringConfigurationValue("Eutelsat_3SS_WS_Username", this)
            {
                OriginalKey = "Eutelsat_3SS_WS_Username"
            };
            Eutelsat_3SS_WS_Password = new StringConfigurationValue("Eutelsat_3SS_WS_Password", this)
            {
                OriginalKey = "Eutelsat_3SS_WS_Password"
            };
            Eutelsat_ProductBase = new StringConfigurationValue("Eutelsat_ProductBase", this)
            {
                ShouldAllowEmpty = true,
                OriginalKey = "Eutelsat_ProductBase"
            };
            Skip3SSCheck = new BooleanConfigurationValue("SKIP_3SS_CHECK", this)
            {
                DefaultValue = false,
                ShouldAllowEmpty = true,
                OriginalKey = "SKIP_3SS_CHECK"
            };
            RightMargin = new NumericConfigurationValue("right_margin", this)
            {
                DefaultValue = 8,
                OriginalKey = "right_margin"
            };
            LeftMargin = new NumericConfigurationValue("left_margin", this)
            {
                DefaultValue = 3,
                OriginalKey = "left_margin"
            };
            TimeMultFactor = new NumericConfigurationValue("time_mult_factor", this)
            {
                DefaultValue = 10000,
                OriginalKey = "time_mult_factor"
            };
            Transaction_Device_Filter = new StringConfigurationValue("Transaction_Device_Filter", this)
            {
                DefaultValue = string.Empty,
                ShouldAllowEmpty = true,
                OriginalKey = "Transaction_Device_Filter"
            };
        }
    }
}