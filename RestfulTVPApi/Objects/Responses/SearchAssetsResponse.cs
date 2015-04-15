using Newtonsoft.Json;
using RestfulTVPApi.Objects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Objects.Responses
{
    public class SearchAssetsResponse : BaseResponse
    {
        [JsonProperty(PropertyName = "assets")]
        public List<AssetInfo> Assets { get; set; }

        [JsonProperty(PropertyName = "total_items")]
        public int TotalItems { get; set; }

    }
}