using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Requests
{
    /// <summary>
    /// description for SlimAssetRequest
    /// </summary>
    public class SlimAssetRequest
    {

        [JsonProperty(PropertyName = "AssetID")]
        public string AssetID { get; set; }

        [JsonProperty(PropertyName = "AssetType")]
        public AssetTypes AssetType { get; set; }

    }

    public enum AssetTypes
    {
        EPG,
        Media,
        NPVR,
        UNKNOWN
    }
}
