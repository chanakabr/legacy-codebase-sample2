using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    public partial class KalturaBookmarkEventThreshold : KalturaOTTObject
    {
        /// <summary>
        /// bookmark transaction type
        /// </summary>
        [DataMember(Name = "transactionType")]
        [JsonProperty("transactionType")]
        [XmlElement(ElementName = "transactionType")]
        public KalturaTransactionType TransactionType { get; set; }

        /// <summary>
        /// event threshold in seconds
        /// </summary>
        [DataMember(Name = "threshold")]
        [JsonProperty("threshold")]
        [XmlElement(ElementName = "threshold")]
        [SchemeProperty(MinInteger = 1)]
        public int Threshold { get; set; }
    }
}