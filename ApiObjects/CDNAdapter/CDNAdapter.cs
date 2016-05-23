using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.CDNAdapter
{
    public class CDNAdapter
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string AdapterUrl { get; set; }
        public string BaseUrl { get; set; }
        public string SystemName { get; set; }        
        public string SharedSecret { get; set; }        

        public List<CDNAdapterSettings> Settings { get; set; }
         
        public CDNAdapter()
        {
            Settings = new List<CDNAdapterSettings>();
        }

        public CDNAdapter(CDNAdapter cdnAdapter)
        {
            this.ID = cdnAdapter.ID;
            this.Name = cdnAdapter.Name;
            this.IsActive = cdnAdapter.IsActive;
            this.AdapterUrl = cdnAdapter.AdapterUrl;
            this.BaseUrl = cdnAdapter.BaseUrl;
            this.SystemName = cdnAdapter.SystemName;
            this.SharedSecret = cdnAdapter.SharedSecret;
            this.Settings = cdnAdapter.Settings;            
        }
    }
}
