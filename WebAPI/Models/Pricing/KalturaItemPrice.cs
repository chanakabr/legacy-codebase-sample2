using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// PPV price details
    /// </summary>
    [Obsolete]
    public partial class KalturaItemPrice : KalturaProductPrice
    {
        /// <summary>
        /// Media file identifier  
        /// </summary>
        [DataMember(Name = "fileId")]
        [JsonProperty("fileId")]
        [XmlElement(ElementName = "fileId")]
        [OldStandardProperty("file_id")]
        public int? FileId { get; set; }

        /// <summary>
        /// PPV price details
        /// </summary>
        [DataMember(Name = "ppvPriceDetails")]
        [JsonProperty("ppvPriceDetails")]
        [XmlArray(ElementName = "ppvPriceDetails", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("ppv_price_details")]
        public List<KalturaPPVItemPriceDetails> PPVPriceDetails { get; set; }
    }
}