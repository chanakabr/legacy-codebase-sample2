using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Users
{

    [DataContract(Name = "Users", Namespace = "")]
    [XmlRoot("Users")]
    public class FavoriteList
    {
        [DataMember(Name = "favorites")]
        [JsonProperty("favorites")]
        public List<Favorite> Favorites;
    }
}