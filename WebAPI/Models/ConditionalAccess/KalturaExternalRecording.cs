using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaExternalRecording : KalturaRecording
    {        

        /// <summary>
        /// External identifier for the recording
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty(PropertyName = "externalId")]
        [XmlElement(ElementName = "externalId")]        
        [SchemeProperty(MinLength = 1, MaxLength = 255, RequiresPermission = (int)RequestType.WRITE, InsertOnly = true)]
        public string ExternalId { get; set; }

        /// <summary>
        /// key/value map field for extra data
        /// </summary>
        [DataMember(Name = "metaData")]
        [JsonProperty("metaData")]
        [XmlElement(ElementName = "metaData")]
        [SchemeProperty(RequiresPermission = (int)RequestType.UPDATE)]
        public SerializableDictionary<string, KalturaStringValue> MetaData { get; set; }

        /// <summary>
        /// Specifies until when the recording is available. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "expiryDate")]
        [JsonProperty("expiryDate")]
        [XmlElement(ElementName = "expiryDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? ExpiryDate { get; set; }
    }
}