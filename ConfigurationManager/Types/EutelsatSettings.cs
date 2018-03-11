using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class EutelsatSettings : ConfigurationValue
    {
        public StringConfigurationValue Eutelsat_CheckTvod_ws;
        public StringConfigurationValue Eutelsat_Transaction_ws;
        public StringConfigurationValue Eutelsat_Subscription_ws;
        public StringConfigurationValue Eutelsat_3SS_WS_Username;
        public StringConfigurationValue Eutelsat_3SS_WS_Password;
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

        internal override bool Validate()
        {
            bool result = true;
            result &= Eutelsat_CheckTvod_ws.Validate();
            result &= Eutelsat_Transaction_ws.Validate();
            result &= Eutelsat_Subscription_ws.Validate();
            result &= Eutelsat_3SS_WS_Username.Validate();
            result &= Eutelsat_3SS_WS_Password.Validate();
            result &= RightMargin.Validate();
            result &= LeftMargin.Validate();
            result &= TimeMultFactor.Validate();

            return result;
        }
    }
}