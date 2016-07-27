using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApiObjects
{
    public class OSSAdapter : OSSAdapterBase
    {
        public bool IsActive { get; set; }
        public string AdapterUrl { get; set; }
        public string ExternalIdentifier { get; set; }        
        public string SharedSecret { get; set; }

        public bool SkipSettings { get; set; }
        public List<OSSAdapterSettings> Settings { get; set; }

        public OSSAdapter()
        {
            Settings = new List<OSSAdapterSettings>();
        }

        public OSSAdapter(OSSAdapter ossAdapter)
        {
            this.ID = ossAdapter.ID;
            this.Name = ossAdapter.Name;
            this.IsActive = ossAdapter.IsActive;
            this.AdapterUrl = ossAdapter.AdapterUrl;
            this.ExternalIdentifier = ossAdapter.ExternalIdentifier;
            this.SharedSecret = ossAdapter.SharedSecret;
            this.Settings = ossAdapter.Settings;
        }


    }
}
