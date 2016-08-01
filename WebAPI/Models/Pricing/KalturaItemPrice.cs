using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// PPV price details
    /// </summary>
    [OldStandard("fileId", "file_id")]
    [OldStandard("ppvPriceDetails", "ppv_price_details")]
    public class KalturaItemPrice : KalturaProductPrice
    {
        /// <summary>
        /// Media file identifier  
        /// </summary>
        [DataMember(Name = "fileId")]
        [JsonProperty("fileId")]
        [XmlElement(ElementName = "fileId")]
        public int? FileId { get; set; }

        /// <summary>
        /// PPV price details
        /// </summary>
        [DataMember(Name = "ppvPriceDetails")]
        [JsonProperty("ppvPriceDetails")]
        [XmlArray(ElementName = "ppvPriceDetails", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaPPVItemPriceDetails> PPVPriceDetails { get; set; }
    }
}