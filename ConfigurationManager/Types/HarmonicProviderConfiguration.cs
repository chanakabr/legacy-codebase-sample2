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
            SmoothCatchup = new StringConfigurationValue("smooth_catchup", this)
            {
                OriginalKey= "smooth_catchup"
            };
            SmoothStartOver = new StringConfigurationValue("smooth_start_over", this)
            {
                OriginalKey = "smooth_start_over"
            };
            HLSStartOver = new StringConfigurationValue("hls_start_over", this)
            {
                OriginalKey = "hls_start_over"
            };
            HLSCatchup = new StringConfigurationValue("hls_catchup", this)
            {
                OriginalKey = "hls_catchup"
            };
            DashCatchup = new StringConfigurationValue("dash_start_over", this)
            {
                OriginalKey = "dash_start_over"
            };
            DashStartOver = new StringConfigurationValue("dash_catchup", this)
            {
                OriginalKey = "dash_catchup"
            };
        }
    }
}