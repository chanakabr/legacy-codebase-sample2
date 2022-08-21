using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    public partial class KalturaListResponse : KalturaOTTObject, IKalturaListResponse
    {
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