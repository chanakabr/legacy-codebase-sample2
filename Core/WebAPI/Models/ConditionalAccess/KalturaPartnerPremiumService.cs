using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Premium service
    /// </summary>
    public partial class KalturaPartnerPremiumService : KalturaOTTObject
    {
        /// <summary>
        /// Service identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public long Id { get; set; }

        /// <summary>
        /// Service name / description
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(ReadOnly = true)]
        public string Name { get; set; }

        /// <summary>
        /// Service name / description
        /// </summary>
        [DataMember(Name = "isApplied")]
        [JsonProperty("isApplied")]
        [XmlElement(ElementName = "isApplied")]
        public bool IsApplied { get; set; }
    }

    public partial class KalturaPartnerPremiumServices : KalturaOTTObject
    {
        /// <summary>
        /// A list of services
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects")]
        [XmlArrayItem("item")]
        public List<KalturaPartnerPremiumService> PremiumServices { get; set; }

        public void ValidateForUpdate()
        {
            if (PremiumServices == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "premiumServices");
            }

            var duplicates = this.PremiumServices.GroupBy(x => x.Id).Count(t => t.Count() >= 2);

            if (duplicates >= 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "premiumServices");
            }
        }
    }
}