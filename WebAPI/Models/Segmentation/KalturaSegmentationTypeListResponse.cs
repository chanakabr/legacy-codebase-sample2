using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// List of segmentation types
    /// </summary>
    [DataContract(Name = "KalturaSegmentationTypeListResponse", Namespace = "")]
    [XmlRoot("KalturaSegmentationTypeListResponse")]
    public partial class KalturaSegmentationTypeListResponse : KalturaListResponse
    {
        /// <summary>
        /// Segmentation Types
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [SchemeProperty()]
        public List<KalturaSegmentationType> SegmentationTypes { get; set; }
    }

    /// <summary>
    /// Filter for segmentation types
    /// </summary>
    public partial class KalturaSegmentationTypeFilter : KalturaFilter<KalturaSegmentationTypeOrder>
    {
        /// <summary>
        /// Comma separated segmentation types identifieridentifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        public string IdIn { get; set; }

        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Ksql { get; set; }

        public override KalturaSegmentationTypeOrder GetDefaultOrderByValue()
        {
            return KalturaSegmentationTypeOrder.NONE;
        }

        public HashSet<long> GetIdIn()
        {
            HashSet<long> hashSet = new HashSet<long>();
            if (!string.IsNullOrEmpty(IdIn))
            {
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value) && !hashSet.Contains(value))
                    {
                        hashSet.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSegmentationTypeFilter.idIn");
                    }
                }
            }

            return hashSet;
        }

        internal bool Validate()
        {
            if (string.IsNullOrEmpty(IdIn) && string.IsNullOrEmpty(Ksql))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "KalturaSegmentationTypeFilter.IdIn", "KalturaSegmentationTypeFilter.Ksql");
            }

            if (!string.IsNullOrEmpty(IdIn) && !string.IsNullOrEmpty(Ksql))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSegmentationTypeFilter.IdIn", "KalturaSegmentationTypeFilter.Ksql");
            }

            return true;
        }
    }

    /// <summary>
    /// Segmentation types order
    /// </summary>
    public enum KalturaSegmentationTypeOrder
    {
        NONE
    }
}