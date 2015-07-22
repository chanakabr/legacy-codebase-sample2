using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Related media request
    /// </summary>
    public class KalturaRelatedMedia : KalturaBaseAssetsRequest
    {
        /// <summary>
        /// Related media types list - possible values:
        /// any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "media_types")]
        [JsonProperty(PropertyName = "media_types")]
        public List<int> media_types { get; set; }
    }
}