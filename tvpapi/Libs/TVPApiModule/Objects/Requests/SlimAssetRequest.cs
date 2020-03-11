using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Context;

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
        public string AssetType { get; set; }

    }

    public enum AssetTypes
    {
        [EnumAsStringValue("EPG")]
        EPG,
        [EnumAsStringValue("MEDIA")]
        MEDIA,
        [EnumAsStringValue("NPVR")]
        NPVR,
        [EnumAsStringValue("UNKNOWN")]
        UNKNOWN
    }
}
