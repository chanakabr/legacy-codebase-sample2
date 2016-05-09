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
        public string BaseUrl { get; set; }
        public string Alias { get; set; }        
        public string SharedSecret { get; set; }        

        public List<CDNAdapterDynamicData> DynamicData { get; set; }
         
        public CDNAdapter()
        {
            DynamicData = new List<CDNAdapterDynamicData>();
        }

        public CDNAdapter(CDNAdapter cdnAdapter)
        {
            this.ID = cdnAdapter.ID;
            this.Name = cdnAdapter.Name;
            this.IsActive = cdnAdapter.IsActive;
            this.BaseUrl = cdnAdapter.BaseUrl;
            this.Alias = cdnAdapter.Alias;
            this.SharedSecret = cdnAdapter.SharedSecret;
            this.DynamicData = cdnAdapter.DynamicData;            
        }
    }
}
