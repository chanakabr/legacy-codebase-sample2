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

        public EutelsatSettings(string key) : base(key)
        {
            Eutelsat_CheckTvod_ws = new StringConfigurationValue("Eutelsat_CheckTvod_ws", this);
            Eutelsat_Transaction_ws = new StringConfigurationValue("Eutelsat_Transaction_ws", this);
            Eutelsat_Subscription_ws = new StringConfigurationValue("Eutelsat_Subscription_ws", this);
            Eutelsat_3SS_WS_Username = new StringConfigurationValue("Eutelsat_3SS_WS_Username", this);
            Eutelsat_3SS_WS_Password = new StringConfigurationValue("Eutelsat_3SS_WS_Password", this);
            Eutelsat_ProductBase = new StringConfigurationValue("Eutelsat_ProductBase", this)
            {
                ShouldAllowEmpty = true
            };
            RightMargin = new NumericConfigurationValue("right_margin", this)
            {
                DefaultValue = 7
            };
            LeftMargin = new NumericConfigurationValue("left_margin", this)
            {
                DefaultValue = -3
            };
            TimeMultFactor = new NumericConfigurationValue("time_mult_factor", this)
            {
                DefaultValue = 10000
            };

        }
    }
}