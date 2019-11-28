using System.Collections.Generic;
using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class TVPApiConfiguration : BaseConfig<TVPApiConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.TVPApiConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public CommaSeparatedConfigurationValue AuthorizationUnsupportedGroupsPlatforms;
        public OfflineFavoriteSyncGroupsConfiguration OfflineFavoriteSyncGroups;


        public BaseValue<bool> ShouldUseNewCache = new BaseValue<bool>("should_use_new_cache", true, true, "Originally in web.config: ShouldUseNewCache");
        public BaseValue<int> CacheLiteDurationInMinutes = new BaseValue<int>("cache_lite_duration_in_minutes", 1440, true, "Originally in web.config: Tvinci.DataLoader.CacheLite.DurationInMinutes");
        public BaseValue<int> OdbcCacheSeconds = new BaseValue<int>("odbc_cache_seconds", 60);
        public BaseValue<int> AuthorizationGroupConfigsTtlSeconds = new BaseValue<int>("authorization_group_configs_ttl_seconds", 86400);
        public BaseValue<int> EPGSearchOffsetDays = new BaseValue<int>("epg_search_offset_days", 7, true, "On old EPG search requests, how many days back/forward from now should we search. " +
                "Originally from GlobalAppSettings.config, key EPGSearchOffsetDays.");
        public BaseValue<string> MainConnectionString = new BaseValue<string>("main_connection_string", null, true, "Originally in web.config: TVinciDBConfig section, but now it is put together.");
        public BaseValue<string> DefaultTechnicalConfigurationFileLocation = new BaseValue<string>("default_technical_configuration_file_location", "DefaultTechnicalConfiguration.config", true, "Location of XML file that will contain all group-platform default values for technical configuration.");
        public BaseValue<string> DefaultMediaConfigurationFileLocation = new BaseValue<string>("default_media_configuration_file_location", "DefaultMediaConfiguration.config", true, "Location of XML file that will contain group-platform default values for media configuration.");
        public BaseValue<string> DefaultSiteConfigurationFileLocation = new BaseValue<string>("default_site_configuration_file_location", "DefaultSiteConfiguration.config", true, "Location of XML file that will contain group-platform default values for site configuration.");
        public BaseValue<string> SecureSiteGuidKey = new BaseValue<string>("secure_site_guid_key", "L3CDpYFfCrGnx5ACoO/Az3oIIt/XeC2dhSmFcB6ckxA=", true, "Originally from GlobalAppSettings.config, key SecureSiteGuidKey");
        public BaseValue<string> SecureSiteGuidIV = new BaseValue<string>("secure_site_guid_iv", "Yn5/n0s8B0yLRvGuhSLRrA==", true, "Originally from GlobalAppSettings.config, key SecureSiteGuidIV");

        /*public TVPApiConfiguration(string key) 
        {
          
            OfflineFavoriteSyncGroups = new OfflineFavoriteSyncGroupsConfiguration("offline_favorite_sync_groups")
            {
                ShouldAllowEmpty = true,
                Description = "Originally in web.config: {group_id}_OfflineFavoriteSync. " +
                "Now it is a comma separted list of groups that should use offline favorite sync"
            };
         
            AuthorizationUnsupportedGroupsPlatforms = new CommaSeparatedConfigurationValue("authorization_unsupported_groups_platforms")
            {
                ShouldAllowEmpty = true,
                Description = "Originally in web.config: Authorization.UnsupportedGroupsPlatforms. " +
                "Comma separated list of {group_id}_{platform}"
            };
           
        }*/

     
    }

    public class OfflineFavoriteSyncGroupsConfiguration : StringConfigurationValue
    {
        HashSet<int> groupIds = null;

        public OfflineFavoriteSyncGroupsConfiguration(string key) : base(key)
        {

        }

        public OfflineFavoriteSyncGroupsConfiguration(string key, ConfigurationValue parent) : base(key, parent)
        {

        }

        internal override bool Validate()
        {
            bool isValid = base.Validate();

            if (!string.IsNullOrEmpty(this.Value))
            {
                string[] values = this.Value.Split(',');
                groupIds = new HashSet<int>();

                foreach (var value in values)
                {
                    int groupId = 0;

                    if (!int.TryParse(value, out groupId))
                    {
                        isValid = false;
                        LogError($"Group Ids should be comma separated list of numbers. Found invalid value {value}", ConfigurationValidationErrorLevel.Failure);
                    }
                    else
                    {
                        groupIds.Add(groupId);
                    }
                }
            }

            return isValid;
        }
        public bool IsOfflineSync(int groupId)
        {
            bool isOffline = false;

            if (groupIds != null)
            {
                isOffline = groupIds.Contains(groupId);
            }

            return isOffline;
        }
    }
}
