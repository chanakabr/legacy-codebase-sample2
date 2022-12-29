using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Partner
{
    public partial  class KalturaOpcPartnerConfiguration : KalturaPartnerConfiguration
    {
        /// <summary>
        /// Reset Password
        /// </summary>
        [DataMember(Name = "resetPassword")]
        [JsonProperty("resetPassword")]
        [XmlElement(ElementName = "resetPassword")]
        public KalturaResetPasswordPartnerConfig ResetPassword { get; set; }
    }
}
