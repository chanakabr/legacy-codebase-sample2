using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class ArrayConfigurationValue<T> : ConfigurationValue where T : ConfigurationValue
    {
        List<T> Objects;

        public ArrayConfigurationValue(string key) : base(key)
        {
            this.Initialize();
        }

        public ArrayConfigurationValue(string key, ConfigurationValue parent) : base(key, parent)
        {
            this.Initialize();
        }

        private void Initialize()
        {
            this.Objects = new List<T>();

            JArray jArray = JArray.Parse(this.ObjectValue.ToString());

            foreach (JObject jObject in jArray)
            {
                
            }
        }
        internal override bool Validate()
        {
            bool result = true;

            foreach (var item in this.Objects)
            {
                result &= item.Validate();
            }

            return result;
        }
    }
}
