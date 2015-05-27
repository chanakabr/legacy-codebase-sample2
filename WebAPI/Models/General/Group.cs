using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models;

namespace WebAPI.Models.General
{
    [Serializable]
    public class Group
    {
        [DataMember(Name="user_secret")]
        [JsonProperty(PropertyName = "user_secret")]
        public string UserSecret { get; set; }

        [JsonProperty(PropertyName = "api_credentials")]
        public Credentials ApiCredentials { get; set; }

        [JsonProperty(PropertyName = "billing_credentials")]
        public Credentials BillingCredentials { get; set; }

        [JsonProperty(PropertyName = "conditinal_access_credentials")]
        public Credentials ConditionalAccessCredentials { get; set; }

        [JsonProperty(PropertyName = "domains_credentials")]
        public Credentials DomainsCredentials { get; set; }

        [JsonProperty(PropertyName = "notifications_credentials")]
        public Credentials NotificationsCredentials { get; set; }

        [JsonProperty(PropertyName = "pricing_credentials")]
        public Credentials PricingCredentials { get; set; }

        [JsonProperty(PropertyName = "social_credentials")]
        public Credentials SocialCredentials { get; set; }

        [JsonProperty(PropertyName = "users_credentials")]
        public Credentials UsersCredentials { get; set; }

        [JsonProperty(PropertyName = "advertising_values_metas")]
        public List<string> AdvertisingValuesMetas { get; set; }

        [JsonProperty(PropertyName = "advertising_values_tags")]
        public List<string> AdvertisingValuesTags { get; set; }

        [JsonProperty(PropertyName = "use_start_date")]
        public bool UseStartDate { get; set; }

        [JsonProperty(PropertyName = "should_support_single_login")]
        public bool ShouldSupportSingleLogin { get; set; }

        [JsonProperty(PropertyName = "should_support_friendly_url")]
        public bool ShouldSupportFriendlyURL { get; set; }

        [JsonIgnore]
        public List<Language> Languages { get; set; }
    }
}