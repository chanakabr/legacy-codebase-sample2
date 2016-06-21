using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Filtering assets
    /// </summary>
    [Serializable]
    public class KalturaAssetFilter : KalturaFilter
    {
        /// <summary>
        /// Filtering condition
        /// </summary>
        public enum KalturaCutWith
        {
            or = 0,
            and = 1
        }

        /// <summary>
        /// Comma separated entities identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty(PropertyName = "idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = true)]
        public string IdIn { get; set; }

        /// <summary>
        /// Reference type of the given IDs
        /// </summary>
        [DataMember(Name = "entityReferenceEqual")]
        [JsonProperty("entityReferenceEqual")]
        [XmlElement(ElementName = "entityReferenceEqual")]
        public KalturaCatalogReferenceBy EntityReferenceEqual { get; set; }

        /// <summary>
        /// order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        [ValidationException(SchemaValidationType.FILTER_SUFFIX)]
        public KalturaAssetOrderBy? OrderBy { get; set; }

        public override object GetDefaultOrderByValue()
        {
            return KalturaAssetOrderBy.NEWEST;
        }
    }
}