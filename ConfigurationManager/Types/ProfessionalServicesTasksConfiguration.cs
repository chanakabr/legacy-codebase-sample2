using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ConfigurationManager
{
    public class ProfessionalServicesTasksConfiguration : BaseConfig<ProfessionalServicesTasksConfiguration>
    {
        public override string TcmKey => null;

        public override string[] TcmPath => null;

        private static readonly Dictionary<string, ProfessionalServicesActionConfiguration> defaultProfessionalServicesActionConfiguration = new Dictionary<string, ProfessionalServicesActionConfiguration>();


        public BaseValue<Dictionary<string, ProfessionalServicesActionConfiguration>> ProfessionalServicesActionConfiguration = 
            new BaseValue<Dictionary<string, ProfessionalServicesActionConfiguration>>(TcmObjectKeys.ProfessionalServicesTasksConfiguration, defaultProfessionalServicesActionConfiguration, true);
    }


    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class ProfessionalServicesActionConfiguration
    {
        [JsonProperty("DllLocation")]
        public string DllLocation
        {
            get;
            set;
        }

        [JsonProperty("Type")]
        public string Type
        {
            get;
            set;
        }

        [JsonProperty("HandlerUrl")]
        public string HandlerUrl
        {
            get;
            set;
        }
    }

}
