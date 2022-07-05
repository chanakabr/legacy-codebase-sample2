using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;
using WebAPI.Managers.Scheme;
using WebAPI.Exceptions;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaPurchaseSession : KalturaPurchase
    {
        /// <summary>
        /// Preview module identifier (relevant only for subscription)
        /// </summary>
        [DataMember(Name = "previewModuleId")]
        [JsonProperty("previewModuleId")]
        [XmlElement(ElementName = "previewModuleId")]
        public int? PreviewModuleId { get; set; }
    }
}