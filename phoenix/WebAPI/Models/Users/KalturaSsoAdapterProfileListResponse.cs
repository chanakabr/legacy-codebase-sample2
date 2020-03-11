using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// ssoAdapterProfile list
    /// </summary>
    [DataContract(Name = "SSOAdapters", Namespace = "")]
    [XmlRoot("SSOAdapters")]
    public partial class KalturaSSOAdapterProfileListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of payment-gateway profiles
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaSSOAdapterProfile> SSOAdapters { get; set; }
    }
}