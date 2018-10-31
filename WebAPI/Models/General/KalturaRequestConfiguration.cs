using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    public enum KalturaSkipOptions
    {
        No = 0,
        // Skip current request if previous Request has an error
        Previous = 1,
        // Skip current request if any of previous Requests had an error
        Any = 2
    }

    /// <summary>
    /// Define client request optional configurations
    /// </summary>
    public partial class KalturaRequestConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// Impersonated partner id
        /// </summary>
        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        public int? PartnerID { get; set; }

        /// <summary>
        /// Impersonated user id
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty("userId")]
        [XmlElement(ElementName = "userId")]
        public int? UserID { get; set; }

        /// <summary>
        /// Content language
        /// </summary>
        [DataMember(Name = "language")]
        [JsonProperty("language")]
        [XmlElement(ElementName = "language")]
        public string Language { get; set; }

        /// <summary>
        /// Currency to be used
        /// </summary>
        [DataMember(Name = "currency")]
        [JsonProperty("currency")]
        [XmlElement(ElementName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Kaltura API session
        /// </summary>
        [DataMember(Name = "ks")]
        [JsonProperty("ks")]
        [XmlElement(ElementName = "ks")]
        public string KS { get; set; }

        /// <summary>
        /// Kaltura response profile object
        /// </summary>
        [DataMember(Name = "responseProfile")]
        [JsonProperty("responseProfile")]
        [XmlElement(ElementName = "responseProfile")]
        public KalturaBaseResponseProfile ResponseProfile { get; set; }

        /// <summary>
        /// Abort all following requests if current request has an error
        /// </summary>
        [DataMember(Name = "abortAllOnError")]
        [JsonProperty("abortAllOnError")]
        [XmlElement(ElementName = "abortAllOnError")]
        public bool AbortAllOnError { get; set; }

        /// <summary>
        /// Skip current request according to skip option
        /// </summary>
        [DataMember(Name = "skipOnOrror")]
        [JsonProperty("skipOnOrror")]
        [XmlElement(ElementName = "skipOnOrror")]
        public KalturaSkipOptions SkipOnOrror { get; set; }

        public KalturaRequestConfiguration()
        {
            AbortAllOnError = false;
            SkipOnOrror = KalturaSkipOptions.No;
        }
    }
}