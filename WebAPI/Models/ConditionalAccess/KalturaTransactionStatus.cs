using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;
using WebAPI.Managers.Scheme;
using WebAPI.Exceptions;

namespace WebAPI.Models.ConditionalAccess
{
    public enum KalturaTransactionAdapterStatus
    {
        OK = 0,
        PENDING = 1,
        FAILED = 2
    }

    public class KalturaTransactionStatus : KalturaOTTObject
    {
        /// <summary>
        /// Payment gateway adapter application state for the transaction to update
        /// </summary>
        [DataMember(Name = "adapterTransactionStatus")]
        [JsonProperty("adapterTransactionStatus")]
        [XmlElement(ElementName = "adapterTransactionStatus")]
        public KalturaTransactionAdapterStatus AdapterStatus { get; set; }

        /// <summary>
        /// External transaction identifier
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty("externalId")]
        [XmlElement(ElementName = "externalId")]
        public string ExternalId { get; set; }

        /// <summary>
        /// Payment gateway transaction status
        /// </summary>
        [DataMember(Name = "externalStatus")]
        [JsonProperty("externalStatus")]
        [XmlElement(ElementName = "externalStatus")]
        public string ExternalStatus { get; set; }

        /// <summary>
        /// Payment gateway message
        /// </summary>
        [DataMember(Name = "externalMessage")]
        [JsonProperty("externalMessage")]
        [XmlElement(ElementName = "externalMessage")]
        public string ExternalMessage { get; set; }

        /// <summary>
        /// The reason the transaction failed
        /// </summary>
        [DataMember(Name = "failReason")]
        [JsonProperty("failReason")]
        [XmlElement(ElementName = "failReason")]
        public int FailReason { get; set; }
    }
}