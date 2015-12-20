using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class OSSAdapterSettings
    {
        public string key { get; set; }
        public string value { get; set; }       

        public OSSAdapterSettings()
        {
        }

        public OSSAdapterSettings(string key, string value)
        {            
            this.key = key;
            this.value = value;
        }
    }
}
