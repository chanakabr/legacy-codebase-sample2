using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Exceptions;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaLicensedUrlRecordingRequest : KalturaLicensedUrlBaseRequest
    {
        /// <summary>
        /// The file type for the URL
        /// </summary>
        [DataMember(Name = "fileType")]
        [JsonProperty("fileType")]
        [XmlElement(ElementName = "fileType")]
        public string FileType { get; set; }
    }
}