using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public class KalturaAssetStruct : KalturaOTTObject
    {
        /// <summary>
        /// Asset Struct id 
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]        
        public long Id { get; set; }

        /// <summary>
        /// Asset struct name for the partner
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name", IsNullable = true)]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// Asset Struct system name for the partner
        /// </summary>
        [DataMember(Name = "systemName")]
        [JsonProperty("systemName")]
        [XmlElement(ElementName = "systemName", IsNullable = true)]
        public string SystemName { get; set; }

        /// <summary>
        ///  Is the Asset Struct protected by the system
        /// </summary>
        [DataMember(Name = "isProtected")]
        [JsonProperty("isProtected")]
        [XmlElement(ElementName = "isProtected", IsNullable = true)]
        [SchemeProperty(RequiresPermission = (int)RequestType.WRITE)]
        public bool? IsProtected { get; set; }

        /// <summary>
        /// A list of comma separated meta ids associated with this asset struct, returned according to the order.
        /// </summary>
        [DataMember(Name = "metaIds")]
        [JsonProperty("metaIds")]
        [XmlElement(ElementName = "metaIds", IsNullable = true)]
        public string MetaIds { get; set; }

        /// <summary>
        /// Specifies when was the Asset Struct was created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the Asset Struct last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        public bool ValidateMetaIds()
        {
            if (!string.IsNullOrEmpty(MetaIds))
            {
                string[] stringValues = MetaIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (!long.TryParse(stringValue, out value) || value < 1)
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetStruct.metaIds");
                    }
                }
            }

            return true;
        }
    }
}