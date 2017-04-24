using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Search history wrapper
    /// </summary>
    [Serializable]
    public class KalturaSearchHistoryListResponse : KalturaListResponse
    {
        /// <summary>
        /// KalturaSearchHistory Models
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaSearchHistory> Objects
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Search history info
    /// </summary>
    [Serializable]
    public class KalturaSearchHistory : KalturaOTTObject
    {
        /// <summary>
        /// Search name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(ReadOnly = true)]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Filter
        /// </summary>
        [DataMember(Name = "filter")]
        [JsonProperty(PropertyName = "filter")]
        [XmlElement(ElementName = "filter")]
        [SchemeProperty(ReadOnly = true)]
        public string Filter
        {
            get;
            set;
        }

        /// <summary>
        /// Search language
        /// </summary>
        [DataMember(Name = "language")]
        [JsonProperty(PropertyName = "language")]
        [XmlElement(ElementName = "language")]
        [SchemeProperty(ReadOnly = true)]
        public string Language
        {
            get;
            set;
        }

        /// <summary>
        /// Search date
        /// </summary>
        [DataMember(Name = "searchDate")]
        [JsonProperty(PropertyName = "searchDate")]
        [XmlElement(ElementName = "searchDate")]
        [SchemeProperty(ReadOnly = true)]
        public long SearchDate
        {
            get;
            set;
        }

        /// <summary>
        /// Kaltura OTT Service
        /// </summary>
        [DataMember(Name = "service")]
        [JsonProperty(PropertyName = "service")]
        [XmlElement(ElementName = "service")]
        [SchemeProperty(ReadOnly = true)]
        public string Service
        {
            get;
            set;
        }

        /// <summary>
        /// Kaltura OTT Service Action
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty(PropertyName = "action")]
        [XmlElement(ElementName = "action")]
        [SchemeProperty(ReadOnly = true)]
        public string Action
        {
            get;
            set;
        }

        /// <summary>
        /// Unique Device ID
        /// </summary>
        [DataMember(Name = "deviceId")]
        [JsonProperty(PropertyName = "deviceId")]
        [XmlElement(ElementName = "deviceId")]
        [SchemeProperty(ReadOnly = true)]
        public string DeviceId
        {
            get;
            set;
        }

    }
}