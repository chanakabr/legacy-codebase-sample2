using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;

namespace REAdapter.Models
{
    public class RefreshTokenRequest
    {
        [DataMember]
        [JsonProperty("grant_type")]
        public string GrantType { get; set; }

        [DataMember]
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [DataMember]
        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        [DataMember]
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        public string ToPostData()
        {
            return string.Format("grant_type={0}&client_id={1}&client_secret={2}&refresh_token={3}",
                !string.IsNullOrEmpty(this.GrantType) ? this.GrantType : string.Empty,         // {0}
                !string.IsNullOrEmpty(this.ClientId) ? this.ClientId : string.Empty,           // {1} 
                !string.IsNullOrEmpty(this.ClientSecret) ? this.ClientSecret : string.Empty,   // {2}
                !string.IsNullOrEmpty(this.RefreshToken) ? this.RefreshToken : string.Empty);  // {3}
        }
    }
}