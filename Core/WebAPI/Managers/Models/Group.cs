using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WebAPI.Managers.Models
{
    [Serializable]
    public class Group
    {
        private const string TOKEN_KEY_FORMAT = "token_{0}";
        private const string APP_TOKEN_KEY_FORMAT = "app_token_{0}";
        private const string USER_SESSIONS_KEY_FORMAT = "sessions_{0}";
        private const string REVOKED_KS_KEY_FORMAT = "r_ks_{0}";
        private const string UPLOAD_TOKEN_KEY_FORMAT = "upload_token_{0}";
        private const string REVOKED_SESSION_KEY_FORMAT = "r_session_{0}";

        [DataMember(Name = "user_secret")]
        [JsonProperty(PropertyName = "user_secret")]
        public string UserSecret { get; set; }

        [JsonProperty(PropertyName = "user_secret_fallback")]
        public string UserSecretFallback { get; set; }

        [JsonProperty(PropertyName = "user_secret_fallback_expiry_epoch")]
        public long UserSecretFallbackExpiryEpoch { get; set; } 

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

        [JsonProperty(PropertyName = "should_check_device_in_domain")]
        public bool ShouldCheckDeviceInDomain { get; set; } = false;

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

        [JsonProperty("upload_token_key_format")]
        public string UploadTokenKeyFormat { get; set; }

        [JsonProperty("upload_token_expiry_seconds")]
        public int UploadTokenExpirySeconds { get; set; }

        [JsonProperty("users_sessions_key_format")]
        public string UserSessionsKeyFormat { get; set; }

        [JsonProperty("revoked_ks_key_format")]
        public string RevokedKsKeyFormat { get; set; }
        
        [JsonProperty("revoked_session_key_format")]
        public string RevokedSessionKeyFormat { get; set; }

        [JsonProperty("revoked_ks_max_ttl_seconds")]
        public int RevokedKsMaxTtlSeconds { get; set; }

        [JsonProperty("account_private_key")]
        public string AccountPrivateKey { get; set; }

        [JsonProperty("media_prep_account_id")]
        public int MediaPrepAccountId { get; set; }

        [JsonProperty("media_prep_account_secret")]
        public string MediaPrepAccountSecret { get; set; }

        [JsonProperty("fairplay_certificate")]
        public string FairplayCertificate { get; set; }

        [JsonProperty("udrm_url")]
        public string UDrmUrl { get; set; }

        [JsonProperty("is_refresh_token_enabled")]
        public bool IsRefreshTokenEnabled { get; set; }

        [JsonProperty("enforce_groups_secret")]
        public bool EnforceGroupsSecret { get; set; }

        /// <summary>
        /// Obsolete - should be taken directly from catalog group object!
        /// </summary>
        [JsonProperty("languages")]
        public List<Language> Languages { get; set; }

        [JsonProperty("apptoken_user_validation_disabled")]
        public bool ApptokenUserValidationDisabled { get; set; }

        internal void SetDefaultValues()
        {
            this.UserSecret = Guid.NewGuid().ToString().Replace("-", "");
            this.UseStartDate = true;
            this.GetOnlyActiveAssets = true;
            this.ShouldSupportSingleLogin = false;
            this.TokenKeyFormat = TOKEN_KEY_FORMAT;
            this.RefreshTokenExpirationSeconds = 1728000;
            this.IsRefreshTokenExtendable = false;
            this.IsSwitchingUsersAllowed = true;
            this.AppTokenKeyFormat = APP_TOKEN_KEY_FORMAT;
            this.UserSessionsKeyFormat = USER_SESSIONS_KEY_FORMAT;
            this.RevokedKsKeyFormat = REVOKED_KS_KEY_FORMAT;
            this.UploadTokenKeyFormat = UPLOAD_TOKEN_KEY_FORMAT;
            this.RevokedSessionKeyFormat = REVOKED_SESSION_KEY_FORMAT;
            this.IsRefreshTokenEnabled = false;
            this.ShouldCheckDeviceInDomain = true;
            this.EnforceGroupsSecret = false;

            if (this.AppTokenSessionMaxDurationSeconds > this.KSExpirationSeconds)
            {
                this.RevokedKsMaxTtlSeconds = this.AppTokenSessionMaxDurationSeconds;
            }
            else
            {
                this.RevokedKsMaxTtlSeconds = (int)this.KSExpirationSeconds;
            }
        }
    }
}