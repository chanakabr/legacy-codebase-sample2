using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    public partial class KalturaSSOAdapterProfileInvoke : KalturaOTTObject
    {
        /// <summary>
        /// SSO Adapter Profile Invoke Response
        /// </summary>
        [DataMember(Name = "sSOAdapterProfileInvoke")]
        [JsonProperty("sSOAdapterProfileInvoke")]
        [XmlArray(ElementName = "sSOAdapterProfileInvoke", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaKeyValue> Response { get; set; }
    }
}
