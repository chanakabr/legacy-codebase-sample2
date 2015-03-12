using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class Region
    {

        [JsonProperty(PropertyName = "id")]
        public long ID { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "external_id")]
        public string ExternalID { get; set; }

        [JsonProperty(PropertyName = "is_default")]
        public bool IsDefault { get; set; }

        public Region(TVPPro.SiteManager.TvinciPlatform.api.Region region)
        {
            if (region != null)
            {
                ID = region.id;
                Name = region.name;
                ExternalID = region.externalId;
                IsDefault = region.isDefault;
            }
        }

    }
}
