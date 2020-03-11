using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class DrmAdapter
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string AdapterUrl { get; set; }
        public string ExternalIdentifier { get; set; }
        public string SharedSecret { get; set; }
        public string Settings { get; set; }

        public DrmAdapter()
        {
        }

        public DrmAdapter(DrmAdapter drmAdapter)
        {
            this.ID = drmAdapter.ID;
            this.Name = drmAdapter.Name;
            this.IsActive = drmAdapter.IsActive;
            this.AdapterUrl = drmAdapter.AdapterUrl;
            this.ExternalIdentifier = drmAdapter.ExternalIdentifier;
            this.SharedSecret = drmAdapter.SharedSecret;
            this.Settings = drmAdapter.Settings;
        }
    }
}
