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
    public class KalturaAssetHistoryCleanFilter : KalturaFilter<KalturaAssetHistoryOrderBy>
    {
        public override KalturaAssetHistoryOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetHistoryOrderBy.NONE;
        }

        /// <summary>
        /// Asset type to search within.
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "typeEqual")]
        [JsonProperty(PropertyName = "typeEqual")]
        [XmlElement(ElementName = "typeEqual", IsNullable = true)]
        public string TypeEqual { get; set; }

        /// <summary>
        /// Comma separated list of asset identifiers.
        /// </summary>
        [DataMember(Name = "assetIdIn")]
        [JsonProperty(PropertyName = "assetIdIn")]
        [XmlElement(ElementName = "assetIdIn", IsNullable = true)]
        public string AssetIdIn { get; set; }

        internal List<string> getAssetIdIn()
        {
            if (AssetIdIn == null)
                return null;

            return AssetIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}