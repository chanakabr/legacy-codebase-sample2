using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Favorite list
    /// </summary>
    [DataContract(Name = "favorites", Namespace = "")]
    [XmlRoot("favorites")]
    public class KalturaFavoriteListResponse : KalturaListResponse
    {
        /// <summary> 
        /// A list of favorites
        /// </summary>
        [DataMember(Name = "favorites")]
        [JsonProperty("favorites")]
        [XmlArray(ElementName = "favorites")]
        [XmlArrayItem("item")] 
        public List<KalturaFavorite> Favorites { get; set; }
    }
}