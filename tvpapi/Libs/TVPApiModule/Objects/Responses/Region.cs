using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.TvinciPlatform.api;

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

        [JsonProperty(PropertyName = "data")]
        public Dictionary<int, int> Data { get; set; }

        public Region(TVPPro.SiteManager.TvinciPlatform.api.Region region)
        {
            if (region != null)
            {
                ID = region.id;
                Name = region.name;
                ExternalID = region.externalId;
                IsDefault = region.isDefault;
                if (region.linearChannels != null)
                {   
                    Data = new Dictionary<int, int>();
                    foreach (KeyValuePair channel in region.linearChannels)
                    {
                        Data.Add(int.Parse(channel.key), int.Parse(channel.value));
                    }
                }
            }
        }

    }
}
