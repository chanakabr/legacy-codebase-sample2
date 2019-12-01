using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class ProfessionalServicesTasksConfiguration : BaseConfig<ProfessionalServicesTasksConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.ProfessionalServicesTasksConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public ProfessionalServicesActionConfiguration ProfessionalServicesActionConfiguration = new ProfessionalServicesActionConfiguration();

    }


        public class ProfessionalServicesActionConfiguration : BaseConfig<ProfessionalServicesActionConfiguration>
        {

            public BaseValue<string> DllLocation = new BaseValue<string>("DllLocation", null);
            public BaseValue<string> Type = new BaseValue<string>("Type", null);

            public override string TcmKey => TcmObjectKeys.ProfessionalServicesActionConfiguration;

            public override string[] TcmPath => new string[] { TcmObjectKeys.ProfessionalServicesTasksConfiguration, TcmKey };
        }
    
}
