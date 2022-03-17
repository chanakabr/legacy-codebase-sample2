using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaImageFilter : KalturaFilter<KalturaImageOrderBy>
    {
        /// <summary>
        /// IDs to filter by
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(DynamicMinInt = 1)]
        public string IdIn { get; set; }

        /// <summary>
        /// ID of the object the is related to, to filter by
        /// </summary>
        [DataMember(Name = "imageObjectIdEqual")]
        [JsonProperty("imageObjectIdEqual")]
        [XmlElement(ElementName = "imageObjectIdEqual")]
        public long? ImageObjectIdEqual { get; set; }

        /// <summary>
        /// Type of the object the image is related to, to filter by
        /// </summary>
        [DataMember(Name = "imageObjectTypeEqual")]
        [JsonProperty("imageObjectTypeEqual")]
        [XmlElement(ElementName = "imageObjectTypeEqual")]
        public KalturaImageObjectType? ImageObjectTypeEqual { get; set; }

        /// <summary>
        /// Filter images that are default on at least on image type or not default at any
        /// </summary>
        [DataMember(Name = "isDefaultEqual")]
        [JsonProperty("isDefaultEqual")]
        [XmlElement(ElementName = "isDefaultEqual", IsNullable = true)]
        public bool? IsDefaultEqual { get; set; }

        /// <summary>
        /// Comma separated imageObject ids list	
        /// </summary>
        [DataMember(Name = "imageObjectIdIn")]
        [JsonProperty("imageObjectIdIn")]
        [XmlElement(ElementName = "imageObjectIdIn")]
        public string ImageObjectIdIn { get; set; }

        public override KalturaImageOrderBy GetDefaultOrderByValue()
        {
            return KalturaImageOrderBy.NONE;
        }
    }
}