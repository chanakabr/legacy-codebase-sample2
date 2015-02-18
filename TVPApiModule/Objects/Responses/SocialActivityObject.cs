using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVPApiModule.Objects.Responses
{
    public class SocialActivityObject
    {
        public int asset_id { get; set; }        
        public string object_id { get; set; }        
        public eAssetType asset_type { get; set; }
        public string asset_name { get; set; }
        public string pic_url { get; set; }
    }
}
