using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Xml.Serialization;
using ApiLogic.Api.Managers;
using ApiObjects;
using WebAPI.Clients;
using WebAPI.Models.General;
using WebAPI.Managers.Models;
using WebAPI.Exceptions;

namespace WebAPI.Models.Partner
{
    public partial class KalturaSecurityPartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// Encryption config
        /// </summary>
        [DataMember(Name = "encryption")]
        [JsonProperty("encryption")]
        [XmlElement(ElementName = "encryption", IsNullable = true)]
        public KalturaDataEncryption Encryption { get; set; }

        protected override KalturaPartnerConfigurationType ConfigurationType => KalturaPartnerConfigurationType.Security;

        internal override bool Update(int groupId)
        {
            if (Encryption?.Username == null) throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "encryption.username");

            var updaterId = KS.GetContextData().UserId.Value; // never null actually

            ClientUtils.GetResponseStatusFromWS((SecurityPartnerConfig partnerConfig) => 
                PartnerConfigurationManager.UpdateSecurityConfig(groupId, partnerConfig, updaterId), this);

            return true;
        }
    }

    public partial class KalturaDataEncryption : KalturaOTTObject
    {
        /// <summary>
        /// Username encryption config
        /// </summary>
        [DataMember(Name = "username")]
        [JsonProperty("username")]
        [XmlElement(ElementName = "username", IsNullable = true)]
        public KalturaEncryption Username { get; set; }
    }

    public partial class KalturaEncryption : KalturaOTTObject
    {
        /// <summary>
        /// Encryption type
        /// </summary>
        [DataMember(Name = "encryptionType")]
        [JsonProperty("encryptionType")]
        [XmlElement(ElementName = "encryptionType")]
        public KalturaEncryptionType EncryptionType { get; set; }
    }

    public enum KalturaEncryptionType
    {
        AES256 = 1
    }
}
