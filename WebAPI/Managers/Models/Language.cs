using Newtonsoft.Json;
using System;

namespace WebAPI.Managers.Models
{
    [Serializable]
    public class Language
    {
        [JsonProperty()]
        public int Id { get; set; }

        [JsonProperty()]
        public string Name { get; set; }

        [JsonProperty()]
        public string Code { get; set; }

        [JsonProperty()]
        public string Direction { get; set; }

        [JsonProperty()]
        public bool IsDefault { get; set; }
    }
}