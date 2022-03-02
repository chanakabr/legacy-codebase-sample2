using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    [Serializable]
    public partial class KalturaNotification : KalturaOTTObject
    {
        [DataMember(Name = "object")]
        [JsonProperty(PropertyName = "object")]
        [XmlElement(ElementName = "object")]
        public KalturaOTTObject eventObject
        {
            get;
            set;
        }

        [DataMember(Name = "eventType")]
        [JsonProperty(PropertyName = "eventType")]
        [XmlElement(ElementName = "eventType")]
        public KalturaEventAction? eventType
        {
            get;
            set;
        }

        [DataMember(Name = "eventObjectType")]
        [JsonProperty(PropertyName = "eventObjectType")]
        [XmlElement(ElementName = "eventObjectType")]
        public string eventObjectType
        {
            get;
            set;
        }

        [DataMember(Name = "systemName")]
        [JsonProperty(PropertyName = "systemName")]
        [XmlElement(ElementName = "systemName")]
        public string systemName
        {
            get;
            set;
        }

        [DataMember(Name = "partnerId")]
        [JsonProperty(PropertyName = "partnerId")]
        [XmlElement(ElementName = "partnerId")]
        public int partnerId
        {
            get;
            set;
        }

        [DataMember(Name = "userIp")]
        [JsonProperty(PropertyName = "userIp")]
        [XmlElement(ElementName = "userIp", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public string UserIp
        {
            get;
            set;
        }

        [DataMember(Name = "sequenceId")]
        [JsonProperty(PropertyName = "sequenceId")]
        [XmlElement(ElementName = "sequenceId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public string SequenceId
        {
            get;
            set;
        }

        [DataMember(Name = "Id")]
        [JsonProperty(PropertyName = "Id")]
        [XmlElement(ElementName = "Id", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public string Id { get; internal set; }

        [DataMember(Name = "userId")]
        [JsonProperty(PropertyName = "userId")]
        [XmlElement(ElementName = "userId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? UserId { get; internal set; }

        [DataMember(Name = "udid")]
        [JsonProperty(PropertyName = "udid")]
        [XmlElement(ElementName = "udid", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public string Udid { get; internal set; }

        [DataMember(Name = "context")]
        [JsonProperty(PropertyName = "context")]
        [XmlElement(ElementName = "context", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public KalturaEventContext Context { get; set; }

        /// <summary>
        /// Create Date
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }
    }
}