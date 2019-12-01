using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class HarmonicProviderConfiguration : BaseConfig<HarmonicProviderConfiguration>
    {

        public override string TcmKey => TcmObjectKeys.HarmonicProviderConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<string> SmoothCatchup = new BaseValue<string>("smooth_catchup", null);
        public BaseValue<string> SmoothStartOver = new BaseValue<string>("smooth_start_over", null);
        public BaseValue<string> HLSStartOver = new BaseValue<string>("smooth_start_over", null);
        public BaseValue<string> HLSCatchup = new BaseValue<string>("dash_catchup", null);
        public BaseValue<string> DashCatchup = new BaseValue<string>("dash_catchup", null);
        public BaseValue<string> DashStartOver = new BaseValue<string>("dash_start_over", null);

    }
}