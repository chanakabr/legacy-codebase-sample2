using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

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
        public Dictionary<long, int> Data { get; set; }

        public Region(ApiObjects.Region region)
        {
            if (region != null)
            {
                ID = region.id;
                Name = region.name;
                ExternalID = region.externalId;
                IsDefault = region.isDefault;
                Data = region.linearChannels?.ToDictionary(x => x.Key, x => x.Value);
            }
        }

    }
}
