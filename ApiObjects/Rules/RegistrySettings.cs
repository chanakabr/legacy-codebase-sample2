using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Rules
{
    public class RegistrySettings
    {
        public string key { get; set; }
        public string value { get; set; }

        public RegistrySettings()
        {
        }

        public RegistrySettings(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
