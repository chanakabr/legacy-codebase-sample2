using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class CDVRAdapterSettings
    {
        public string key { get; set; }
        public string value { get; set; }       

        public CDVRAdapterSettings()
        {
        }

        public CDVRAdapterSettings(string key, string value)
        {             
            this.key = key;
            this.value = value; 
        }
    }
}
