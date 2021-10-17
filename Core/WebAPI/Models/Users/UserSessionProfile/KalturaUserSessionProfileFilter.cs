using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users.UserSessionProfile
{
    /// <summary>
    /// User Session Profile filter
    /// </summary>
    public partial class KalturaUserSessionProfileFilter : KalturaFilter<KalturaUserSessionProfileFilterOrderBy>
    {
        /// <summary>
        /// UserSessionProfile identifier to filter by
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual")]
        [SchemeProperty(MinLong = 1, IsNullable = true)]
        public long? IdEqual { get; set; }

        public override KalturaUserSessionProfileFilterOrderBy GetDefaultOrderByValue()
        {
            return KalturaUserSessionProfileFilterOrderBy.ID_ASC;
        }
    }

    public enum KalturaUserSessionProfileFilterOrderBy
    {
        ID_ASC
    }
}