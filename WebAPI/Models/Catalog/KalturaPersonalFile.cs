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
    public partial class KalturaPersonalFile : KalturaOTTObject
    {
        /// <summary>
        /// File unique identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        public int? Id
        {
            get;
            set;
        }

        /// <summary>
        /// User price for the item* is smaller than standard (full) price
        /// </summary>
        [DataMember(Name = "discounted")]
        [JsonProperty(PropertyName = "discounted")]
        [XmlElement(ElementName = "discounted")]
        public bool? Discounted
        {
            get;
            set;
        }


        /// <summary>
        /// A date when the current offer (discounted price) will expire for the item
        /// </summary>
        [DataMember(Name = "offer")]
        [JsonProperty(PropertyName = "offer")]
        [XmlElement(ElementName = "offer")]
        public string Offer
        {
            get;
            set;
        }

        /// <summary>
        /// A date when the current offer (discounted price) will expire for the item
        /// </summary>
        [DataMember(Name = "entitled")]
        [JsonProperty(PropertyName = "entitled")]
        [XmlElement(ElementName = "entitled")]
        public bool? Entitled
        {
            get;
            set;
        }
    }
}