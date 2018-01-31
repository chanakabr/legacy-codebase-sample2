using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Exceptions;

namespace WebAPI.Models.Catalog
{
    public class KalturaRatio : KalturaOTTObject
    {
        /// <summary>
        /// ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Height
        /// </summary>
        [DataMember(Name = "height")]
        [JsonProperty(PropertyName = "height")]
        [XmlElement(ElementName = "height")]
        [SchemeProperty(MinInteger = 1)]
        public int Height { get; set; }

        /// <summary>
        /// Width
        /// </summary>
        [DataMember(Name = "width")]
        [JsonProperty(PropertyName = "width")]
        [XmlElement(ElementName = "width")]
        [SchemeProperty(MinInteger = 1)]
        public int Width { get; set; }

        /// <summary>
        /// Accepted error margin precentage of an image uploaded for this ratio
        /// 0 - no validation, everything accepted
        /// </summary>
        [DataMember(Name = "acceptedErrorMarginPrecentage")]
        [JsonProperty(PropertyName = "acceptedErrorMarginPrecentage")]
        [XmlElement(ElementName = "acceptedErrorMarginPrecentage")]
        [SchemeProperty(MinInteger = 0, MaxInteger = 100)]
        public int AcceptedErrorMarginPrecentage { get; set; }

        internal void Validate()
        {
            if (Height <= 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "ratio.height", 1);
            }

            if (Width <= 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "ratio.width", 1);
            }

            if (AcceptedErrorMarginPrecentage < 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "ratio.acceptedErrorMarginPrecentage", 0);
            }

            if (AcceptedErrorMarginPrecentage > 100)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_VALUE_CROSSED, "ratio.acceptedErrorMarginPrecentage", 100);
            }
        }
    }

    public class KalturaRatioListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of ratios
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaRatio> Ratios { get; set; }
    }
}