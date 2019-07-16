using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.MultiRequest;

namespace WebAPI.Models.General
{
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
        /// Abort the Multireuqset call if any error occurs in one of the requests
        /// </summary>
        [DataMember(Name = "abortOnError")]
        [JsonProperty("abortOnError")]
        [XmlElement(ElementName = "abortOnError")]
        public bool AbortOnError { get; set; }

        /// <summary>
        /// Abort all following requests in Multireuqset if current request has an error
        /// </summary>
        [DataMember(Name = "abortAllOnError")]
        [JsonProperty("abortAllOnError")]
        [XmlElement(ElementName = "abortAllOnError")]
        public bool AbortAllOnError { get; set; }

        /// <summary>
        /// Skip current request according to skip condition
        /// </summary>
        [DataMember(Name = "skipCondition")]
        [JsonProperty("skipCondition")]
        [XmlElement(ElementName = "skipCondition")]
        public KalturaSkipCondition SkipCondition { get; set; }
        
        public KalturaRequestConfiguration()
        {
            AbortAllOnError = false;
            AbortOnError = false;
        }
    }
}