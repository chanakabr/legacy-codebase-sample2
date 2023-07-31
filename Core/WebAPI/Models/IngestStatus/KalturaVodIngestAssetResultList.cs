using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaVodIngestAssetResultList : KalturaOTTObject, IKalturaListResponse
    {
        /// <summary>
        /// list of KalturaVodIngestAssetResult
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaVodIngestAssetResult> Objects { get; set; }

        /// <summary>
        /// Total items
        /// </summary>
        [DataMember(Name = "totalCount")]
        [JsonProperty(PropertyName = "totalCount")]
        [XmlElement(ElementName = "totalCount")]
        [ValidationException(SchemeValidationType.NULLABLE)]
        public int TotalCount { get; set; }

        public string ToJson(Version currentVersion, bool omitObsolete, bool responseProfile = false)
        {
            return base.ToJson(currentVersion, omitObsolete, true);
        }

        public string ToXml(Version currentVersion, bool omitObsolete, bool responseProfile = false)
        {
            return base.ToXml(currentVersion, omitObsolete, true);
        }
    }
}
