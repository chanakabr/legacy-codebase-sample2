using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.CDNAdapter
{
    public class CDNAdapterSettings
    {
        public string key { get; set; }
        public string value { get; set; }       

        public CDNAdapterSettings()
        {
        }

        public CDNAdapterSettings(string key, string value)
        {             
            this.key = key;
            this.value = value; 
        }
    }
}
