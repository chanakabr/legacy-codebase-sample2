using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models;

namespace WebAPI.Managers.Models
{
    [Serializable]
    public class Group
    {
        [DataMember(Name = "user_secret")]
        [JsonProperty(PropertyName = "user_secret")]
        public string UserSecret { get; set; }

        [JsonProperty(PropertyName = "admin_secret")]
        public string AdminSecret { get; set; }

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

        [JsonProperty(PropertyName = "get_only_active_assets")]
        public bool GetOnlyActiveAssets { get; set; }

        [JsonProperty(PropertyName = "should_support_single_login")]
        public bool ShouldSupportSingleLogin { get; set; }

        [JsonProperty(PropertyName = "should_support_friendly_url")]
        public bool ShouldSupportFriendlyURL { get; set; }

        [JsonProperty(PropertyName = "ks_expiration_seconds")]
        public long KSExpirationSeconds { get; set; }

        [JsonProperty(PropertyName = "anonymous_ks_expiration_seconds")]
        public long AnonymousKSExpirationSeconds { get; set; }

        [JsonProperty(PropertyName = "refresh_token_expiration_seconds")]
        public long RefreshTokenExpirationSeconds { get; set; }

        [JsonProperty("is_refresh_token_extendable")]
        public bool IsRefreshTokenExtendable { get; set; }

        [JsonProperty("refresh_expiration_for_pin_login_seconds")]
        public long RefreshExpirationForPinLoginSeconds { get; set; }

        [JsonProperty("is_switching_users_allowed")]
        public bool IsSwitchingUsersAllowed { get; set; }

        [JsonProperty("token_key_format")]
        public string TokenKeyFormat { get; set; }

        [JsonProperty("app_token_key_format")]
        public string AppTokenKeyFormat { get; set; }

        [JsonProperty("app_token_session_max_duration_seconds")]
        public int AppTokenSessionMaxDurationSeconds { get; set; }

        [JsonProperty("app_token_max_expiry_seconds")]
        public int AppTokenMaxExpirySeconds { get; set; }

        [JsonProperty("users_sessions_key_format")]
        public string UserSessionsKeyFormat { get; set; }

        [JsonProperty("revoked_ks_key_format")]
        public string RevokedKsKeyFormat { get; set; }

        [JsonProperty("revoked_ks_max_ttl_seconds")]
        public int RevokedKsMaxTtlSeconds { get; set; }

        [JsonProperty("account_private_key")]
        public string AccountPrivateKey { get; set; }

        [JsonProperty("media_prep_account_id")]
        public int MediaPrepAccountId { get; set; }

        [JsonProperty("fairplay_certificate")]
        public string FairplayCertificate { get; set; }

        [JsonIgnore]
        public List<Language> Languages { get; set; }

        [JsonIgnore]
        public Dictionary<string, Dictionary<long, string>> PermissionItemsRolesMapping { get; set; }

        [JsonIgnore]
        public Dictionary<long, string> RolesIdsNamesMapping { get; set; }
    }
}