using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Users list
    /// </summary>
    [DataContract(Name = "Users", Namespace = "")]
    [XmlRoot("Users")]
    public class KalturaOTTUserArray : KalturaOTTObject
    {
        /// <summary>
        /// A list of users
        /// </summary>
        [DataMember(Name = "users")]
        [JsonProperty("users")]
        [XmlArray(ElementName = "users")]
        [XmlArrayItem("item")] 
        public List<KalturaOTTUser> Users { get; set; }
    }
}