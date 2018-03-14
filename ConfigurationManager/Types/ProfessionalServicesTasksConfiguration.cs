using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class ProfessionalServicesTasksConfiguration : StringConfigurationValue
    {
        private JObject json;

        public ProfessionalServicesTasksConfiguration(string key) : base(key)
        {
            if (!string.IsNullOrEmpty(this.Value))
            {
                json = JObject.Parse(this.Value);
            }
        }

        internal override bool Validate()
        {
            bool result = base.Validate();

            if (json != null)
            {
                foreach (var token in json.Children())
                {
                    try
                    {
                        ProfessionalServicesActionConfiguration action = (token as JProperty).Value.ToObject<ProfessionalServicesActionConfiguration>();
                    }
                    catch (Exception ex)
                    {
                        LogError(string.Format("Could not load action. ex = {0}", ex));
                    }
                }
            }

            return result;
        }

        public JToken GetActionHandler(string action)
        {
            JToken result = null;

            if (this.json != null)
            {
                result = this.json.SelectToken(action);
            }

            return result;
        }

        [Serializable]
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

        }
    }
}
