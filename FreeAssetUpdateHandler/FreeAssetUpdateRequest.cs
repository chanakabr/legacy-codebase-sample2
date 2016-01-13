using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeAssetUpdateHandler
{
    [Serializable]
    public class FreeAssetUpdateRequest
    {
        [JsonProperty("group_id")]
        public int GroupID
        {
            get;
            set;
        }

        [JsonProperty("type", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ApiObjects.eObjectType Type
        {
            get;
            set;
        }

        [JsonProperty("asset_ids", Required = Required.Always)]
        public List<int> AssetIds
        {
            get;
            set;
        }

        [JsonProperty("module_ids", Required = Required.Always)]
        public List<int> ModuleIds
        {
            get;
            set;
        }
    }
}
