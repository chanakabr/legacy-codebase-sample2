using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public class KalturaImageFilter : KalturaFilter<KalturaImageOrderBy>
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
        /// Filter images that are default on atleast on image type or not default at any
        /// </summary>
        [DataMember(Name = "isDefaultEqual")]
        [JsonProperty("isDefaultEqual")]
        [XmlElement(ElementName = "isDefaultEqual", IsNullable = true)]
        public bool? IsDefaultEqual { get; set; }

        public override KalturaImageOrderBy GetDefaultOrderByValue()
        {
            return KalturaImageOrderBy.NONE;
        }

        public List<long> GetIdIn()
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(IdIn))
            {
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaImageFilter.idIn");
                    }
                }
            }

            return list;
        }

        internal void Validate()
        {
            if (!string.IsNullOrEmpty(IdIn) && ImageObjectIdEqual.HasValue && ImageObjectIdEqual != 0 && ImageObjectTypeEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaImageFilter.idIn", "KalturaImageFilter.imageObjectIdEqual");
            }
            if (ImageObjectIdEqual.HasValue && ImageObjectIdEqual != 0 && !ImageObjectTypeEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaImageFilter.imageObjectTypeEqual");
            }
            if (ImageObjectTypeEqual.HasValue && (!ImageObjectIdEqual.HasValue || ImageObjectIdEqual == 0))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaImageFilter.imageObjectIdEqual");
            }
            if (!ImageObjectIdEqual.HasValue && !ImageObjectTypeEqual.HasValue && string.IsNullOrEmpty(IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "KalturaImageFilter.imageObjectIdEqual", "KalturaImageFilter.idIn");
            }
        }
    }

    public enum KalturaImageOrderBy
    {
        NONE
    }
}