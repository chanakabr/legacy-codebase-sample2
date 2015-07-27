using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Channel media request
    /// </summary>
    public class KalturaChannelMedia : KalturaBaseAssetsRequest
    {
        /// <summary>
        /// Required sort option to apply for the identified assets. If omitted – will use channel default ordering.
        /// Possible values: relevancy, a_to_z, z_to_a, views, ratings, votes, newest.
        /// </summary>
        [DataMember(Name = "order_by")]
        [JsonProperty(PropertyName = "order_by")]
        [XmlElement(ElementName = "order_by")]
        public KalturaOrder? order_by { get; set; }
    }
}