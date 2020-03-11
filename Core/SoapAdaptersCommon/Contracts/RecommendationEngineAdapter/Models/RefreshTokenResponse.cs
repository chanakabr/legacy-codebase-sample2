using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;

namespace REAdapter.Models
{
    [DataContract]
    public class RefreshTokenResponse
    {
        [DataMember]
        [JsonProperty("access_token")]
        public string AcceessToken { get; set; }

        [DataMember]
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [DataMember]
        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }
    }
}