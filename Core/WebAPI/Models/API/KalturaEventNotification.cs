using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Household Coupon details
    /// </summary>
    [Serializable]
    public partial class KalturaEventNotification : KalturaOTTObjectSupportNullable
    {
        /// <summary>
        /// Identifier 
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Object identifier 
        /// </summary>
        [DataMember(Name = "objectId")]
        [JsonProperty("objectId")]
        [XmlElement(ElementName = "objectId")]
        [SchemeProperty(MinInteger = 1)]
        public long ObjectId { get; set; }

        /// <summary>
        /// Event object type 
        /// </summary>
        [DataMember(Name = "eventObjectType")]
        [JsonProperty("eventObjectType")]
        [XmlElement(ElementName = "eventObjectType")]
        public string EventObjectType { get; set; }

        /// <summary>
        /// Message 
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty("message")]
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        public KalturaEventNotificationStatus Status { get; set; }

        /// <summary>
        /// Action type
        /// </summary>
        [DataMember(Name = "actionType")]
        [JsonProperty("actionType")]
        [XmlElement(ElementName = "actionType")]
        public string ActionType { get; set; }

        /// <summary>
        /// Create date
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Update date
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }
    }
}