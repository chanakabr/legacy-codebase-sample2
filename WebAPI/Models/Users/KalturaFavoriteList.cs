using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Favorite list
    /// </summary>
    [DataContract(Name = "favorites", Namespace = "")]
    [XmlRoot("favorites")]
    public class KalturaFavoriteList
    {
        /// <summary>
        /// A list of favorites
        /// </summary>
        [DataMember(Name = "favorites")]
        [JsonProperty("favorites")]
        public List<KalturaFavorite> Favorites { get; set; }
    }
}