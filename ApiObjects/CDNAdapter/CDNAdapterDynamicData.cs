using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.CDNAdapter
{
    public class CDNAdapterDynamicData
    {
        public string key { get; set; }
        public string value { get; set; }       

        public CDNAdapterDynamicData()
        {
        }

        public CDNAdapterDynamicData(string key, string value)
        {             
            this.key = key;
            this.value = value; 
        }
    }
}
