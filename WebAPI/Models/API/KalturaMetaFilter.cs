using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Meta filter
    /// </summary>
    public class KalturaMetaFilter : KalturaFilter<KalturaMetaOrderBy>
    {
        /// <summary>
        /// Meta system field name to filter by
        /// </summary>
        [DataMember(Name = "fieldNameEqual")]
        [JsonProperty("fieldNameEqual")]
        [XmlElement(ElementName = "fieldNameEqual")]
        public KalturaMetaFieldName FieldNameEqual { get; set; }

        /// <summary>
        /// Meta system field name to filter by
        /// </summary>
        [DataMember(Name = "fieldNameNotEqual")]
        [JsonProperty("fieldNameNotEqual")]
        [XmlElement(ElementName = "fieldNameNotEqual")]
        public KalturaMetaFieldName FieldNameNotEqual { get; set; }

        /// <summary>
        /// Meta type to filter by
        /// </summary>
        [DataMember(Name = "typeEqual")]
        [JsonProperty("typeEqual")]
        [XmlElement(ElementName = "typeEqual")]
        public KalturaMetaType? TypeEqual { get; set; }

        /// <summary>
        /// Asset type to filter by
        /// </summary>
        [DataMember(Name = "assetTypeEqual")]
        [JsonProperty("assetTypeEqual")]
        [XmlElement(ElementName = "assetTypeEqual")]
        public KalturaAssetType? AssetTypeEqual { get; set; }
        


        public override KalturaMetaOrderBy GetDefaultOrderByValue()
        {
            return KalturaMetaOrderBy.NONE;
        }
    }
}