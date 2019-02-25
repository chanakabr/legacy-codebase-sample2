using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaPersonalListSearchFilter : KalturaBaseSearchAssetFilter
    {
        /// <summary>
        /// Comma separated list of partner list types to search within. 
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "partnerListTypeIn")]
        [JsonProperty("partnerListTypeIn")]
        [SchemeProperty(DynamicMinInt = 0)]
        [XmlElement(ElementName = "partnerListTypeIn", IsNullable = true)]
        public string PartnerListTypeIn { get; set; }

        internal HashSet<int> GetPartnerListTypeIn()
        {
            return this.GetItemsIn<HashSet<int>, int>(PartnerListTypeIn, "KalturaPersonalListSearchFilter.PartnerListTypeIn", false, true);
        }
    }
}