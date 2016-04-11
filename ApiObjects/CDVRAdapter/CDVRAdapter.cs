using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApiObjects
{
    public class CDVRAdapter 
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string AdapterUrl { get; set; }
        public string ExternalIdentifier { get; set; }        
        public string SharedSecret { get; set; }
        public bool DynamicLinksSupport { get; set; } 

        public List<CDVRAdapterSettings> Settings { get; set; }
         
        public CDVRAdapter()
        {
            Settings = new List<CDVRAdapterSettings>();
        }

        public CDVRAdapter(CDVRAdapter cdvrAdapter)
        {
            this.ID = cdvrAdapter.ID;
            this.Name = cdvrAdapter.Name;
            this.IsActive = cdvrAdapter.IsActive;
            this.AdapterUrl = cdvrAdapter.AdapterUrl;
            this.ExternalIdentifier = cdvrAdapter.ExternalIdentifier;
            this.SharedSecret = cdvrAdapter.SharedSecret;
            this.Settings = cdvrAdapter.Settings;
            this.DynamicLinksSupport = cdvrAdapter.DynamicLinksSupport;
        }


    }
}
