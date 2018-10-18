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
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public string IdIn { get; set; }

        public override KalturaSegmentationTypeOrder GetDefaultOrderByValue()
        {
            return KalturaSegmentationTypeOrder.NONE;
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
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSegmentationTypeFilter.idIn");
                    }
                }
            }

            return new List<long>(list);
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