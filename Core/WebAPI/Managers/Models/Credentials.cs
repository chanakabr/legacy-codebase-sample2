using Newtonsoft.Json;
using System;

namespace WebAPI.Managers.Models
{
    [Serializable]
    public class Credentials
    {
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }
    }
}