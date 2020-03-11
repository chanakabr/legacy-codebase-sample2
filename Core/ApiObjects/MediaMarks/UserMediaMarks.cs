using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.MediaMarks
{
    [JsonObject()]
    [Serializable]
    public class UserMediaMarks
    {
        [JsonProperty("mediaMarks")]
        public List<AssetAndLocation> mediaMarks;

        public UserMediaMarks()
        {
            mediaMarks = new List<AssetAndLocation>();
        }
    }

    public class AssetAndLocation
    {
        [JsonProperty("assetId")]
        public int AssetId { get; set; }
        
        [JsonProperty("assetType")]
        public eAssetTypes AssetType { get; set; }

        [JsonProperty("createdAt")]
        public long CreatedAt { get; set; }

        public AssetAndLocation()
        {

        }
    }
}
