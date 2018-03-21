using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class HarmonicProviderConfiguration : ConfigurationValue
    {
        public StringConfigurationValue SmoothCatchup;
        public StringConfigurationValue SmoothStartOver;
        public StringConfigurationValue HLSStartOver;
        public StringConfigurationValue HLSCatchup;
        public StringConfigurationValue DashCatchup;
        public StringConfigurationValue DashStartOver;

        public HarmonicProviderConfiguration(string key) : base(key)
        {
            SmoothCatchup = new StringConfigurationValue("smooth_catchup", this);
            SmoothStartOver = new StringConfigurationValue("smooth_start_over", this);
            HLSStartOver = new StringConfigurationValue("hls_start_over", this);
            HLSCatchup = new StringConfigurationValue("hls_catchup", this);
            DashCatchup = new StringConfigurationValue("dash_start_over", this);
            DashStartOver = new StringConfigurationValue("dash_catchup", this);
        }

        internal override bool Validate()
        {
            bool result = true;
            result &= SmoothCatchup.Validate();
            result &= SmoothStartOver.Validate();
            result &= HLSStartOver.Validate();
            result &= HLSCatchup.Validate();
            result &= DashCatchup.Validate();
            result &= DashStartOver.Validate();

            return result;
        }
    }
}