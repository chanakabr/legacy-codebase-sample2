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

        public List<long> GetIdIn()
        {
            HashSet<long> list = new HashSet<long>();
            if (!string.IsNullOrEmpty(IdIn))
            {
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value) && !list.Contains(value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaImageFilter.idIn");
                    }
                }
            }

            return new List<long>(list);
        }

        public List<long> GetImageObjectIdIn()
        {
            HashSet<long> list = new HashSet<long>();
            if (!string.IsNullOrEmpty(ImageObjectIdIn))
            {
                string[] stringValues = ImageObjectIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value) && !list.Contains(value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaImageFilter.imageObjectIdIn");
                    }
                }
            }

            return new List<long>(list);
        }

        internal void Validate()
        {
            if (!string.IsNullOrEmpty(IdIn))
            {
                if (ImageObjectIdEqual.HasValue && ImageObjectIdEqual != 0 && ImageObjectTypeEqual.HasValue)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaImageFilter.idIn", "KalturaImageFilter.imageObjectIdEqual");
                }
                else if (!string.IsNullOrEmpty(ImageObjectIdIn) && ImageObjectTypeEqual.HasValue)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaImageFilter.imageObjectIdIn", "KalturaImageFilter.imageObjectIdEqual");
                }
            }

            if (ImageObjectIdEqual.HasValue && ImageObjectIdEqual != 0 && !ImageObjectTypeEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaImageFilter.imageObjectTypeEqual");
            }

            if (ImageObjectTypeEqual.HasValue)
            {
                if ((!ImageObjectIdEqual.HasValue || ImageObjectIdEqual == 0) && string.IsNullOrEmpty(ImageObjectIdIn))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, $"{"KalturaImageFilter.imageObjectIdEqual, KalturaImageFilter.imageObjectIdIn"}");
                }
            }

            if (!ImageObjectIdEqual.HasValue && !ImageObjectTypeEqual.HasValue && string.IsNullOrEmpty(IdIn) && string.IsNullOrEmpty(ImageObjectIdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, $"{"KalturaImageFilter.imageObjectIdEqual, KalturaImageFilter.idIn, KalturaImageFilter.imageObjectIdIn"}");
            }

            if (!string.IsNullOrEmpty(ImageObjectIdIn) && !ImageObjectTypeEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaImageFilter.imageObjectIdIn");
            }
        }
    }

    public enum KalturaImageOrderBy
    {
        NONE
    }
}