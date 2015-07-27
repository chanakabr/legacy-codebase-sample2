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
    /// Autocomplete request
    /// </summary>
    public class KalturaAutocompleteRequest : KalturaOTTObject
    {
        /// <summary>
        /// List of asset types to search within.
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "filter_types")]
        [JsonProperty(PropertyName = "filter_types")]
        [XmlElement(ElementName = "filter_types")]
        public List<int> filter_types { get; set; }

        /// <summary>
        ///Search string to look for within the assets’ title only. Search is starts with. White spaces are not ignored.
        /// </summary>
        [DataMember(Name = "query")]
        [JsonProperty(PropertyName = "query")]
        [XmlElement(ElementName = "query")]
        public string query { get; set; }

        /// <summary>
        /// Required sort option to apply for the identified assets. If omitted – will use newest.
        /// Possible values: relevancy, a_to_z, z_to_a, views, ratings, votes, newest.
        /// </summary>
        [DataMember(Name = "order_by")]
        [JsonProperty(PropertyName = "order_by")]
        [XmlElement(ElementName = "order_by")]
        public KalturaOrder? order_by { get; set; }

        /// <summary>
        ///Additional data to return per asset, formatted as a comma-separated array. 
        ///Possible values: images – add the ImageModel to each asset. 
        /// </summary>
        [DataMember(Name = "with")]
        [JsonProperty(PropertyName = "with")]
        [XmlElement(ElementName = "with")]
        public List<KalturaCatalogWith> with { get; set; }

        /// <summary>
        /// <![CDATA[Maximum number of assets to return. Possible range 1 ≤ size ≥ 10. If omitted or not in range – default to 5]]>
        /// </summary>
        [DataMember(Name = "size")]
        [JsonProperty(PropertyName = "size")]
        [XmlElement(ElementName = "size")]
        public int? size { get; set; }
    }
}