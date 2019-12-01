using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class EutelsatSettings : BaseConfig<EutelsatSettings>
    {
        public BaseValue<string> Eutelsat_CheckTvod_ws = new BaseValue<string>("Eutelsat_CheckTvod_ws", null);
        public override string TcmKey => TcmObjectKeys.EutelsatSettings;

        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<string> Eutelsat_Transaction_ws = new BaseValue<string>("Eutelsat_Transaction_ws", null);
        public BaseValue<string> Eutelsat_Subscription_ws = new BaseValue<string>("Eutelsat_Subscription_ws", null);
        public BaseValue<string> Eutelsat_3SS_WS_Username = new BaseValue<string>("Eutelsat_3SS_WS_Username", null);
        public BaseValue<string> Eutelsat_3SS_WS_Password = new BaseValue<string>("Eutelsat_3SS_WS_Password", null);
        public BaseValue<string> Eutelsat_ProductBase = new BaseValue<string>("Eutelsat_ProductBase", null);
        public BaseValue<int> RightMargin = new BaseValue<int>("right_margin", 8);
        public BaseValue<int> LeftMargin = new BaseValue<int>("left_margin", 3);
        public BaseValue<int> TimeMultFactor = new BaseValue<int>("time_mult_factor", 10000);
        public BaseValue<bool> Skip3SSCheck = new BaseValue<bool>("SKIP_3SS_CHECK", false);
        public BaseValue<string> Transaction_Device_Filter = new BaseValue<string>("Transaction_Device_Filter", string.Empty);

    }
}