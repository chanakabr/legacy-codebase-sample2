using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ConfigurationManager
{
    public class TVPApiConfiguration : ConfigurationValue
    {
        public NumericConfigurationValue CacheLiteDurationInMinutes;
        public BooleanConfigurationValue ShouldUseNewCache;
        public OfflineFavoriteSyncGroupsConfiguration OfflineFavoriteSyncGroups;
        public NumericConfigurationValue OdbcCacheSeconds;
        public NumericConfigurationValue AuthorizationGroupConfigsTtlSeconds;
        public CommaSeparatedConfigurationValue AuthorizationUnsupportedGroupsPlatforms;
        public StringConfigurationValue MainConnectionString;
        public StringConfigurationValue DefaultTechnicalConfigurationFileLocation;
        public StringConfigurationValue DefaultMediaConfigurationFileLocation;
        public StringConfigurationValue DefaultSiteConfigurationFileLocation;

        public TVPApiConfiguration(string key) : base(key)
        {
            CacheLiteDurationInMinutes = new NumericConfigurationValue("cache_lite_duration_in_minutes", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = 1440,
                Description = "Originally in web.config: Tvinci.DataLoader.CacheLite.DurationInMinutes"
            };
            ShouldUseNewCache = new BooleanConfigurationValue("should_use_new_cache", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = true,
                Description = "Originally in web.config: ShouldUseNewCache"
            };
            OfflineFavoriteSyncGroups = new OfflineFavoriteSyncGroupsConfiguration("offline_favorite_sync_groups", this)
            {
                ShouldAllowEmpty = true,
                Description = "Originally in web.config: {group_id}_OfflineFavoriteSync. " +
                "Now it is a comma separted list of groups that should use offline favorite sync"
            };
            OdbcCacheSeconds = new NumericConfigurationValue("odbc_cache_seconds", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = 60
            };
            AuthorizationGroupConfigsTtlSeconds = new NumericConfigurationValue("authorization_group_configs_ttl_seconds", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = 86400
            };
            AuthorizationUnsupportedGroupsPlatforms = new CommaSeparatedConfigurationValue("authorization_unsupported_groups_platforms", this)
            {
                ShouldAllowEmpty = true,
                Description = "Originally in web.config: Authorization.UnsupportedGroupsPlatforms. " +
                "Comma separated list of {group_id}_{platform}"
            };
            MainConnectionString = new StringConfigurationValue("main_connection_string", this)
            {
                ShouldAllowEmpty = true,
                Description = "Originally in web.config: TVinciDBConfig section, but now it is put together."
            };
            DefaultTechnicalConfigurationFileLocation = new StringConfigurationValue("default_technical_configuration_file_location", this)
            {
                ShouldAllowEmpty = true,
                Description = "Location of XML file that will contain all group-platform default values for technical configuration.",
                DefaultValue = "DefaultTechnicalConfiguration.config"
            };
            DefaultSiteConfigurationFileLocation = new StringConfigurationValue("default_site_configuration_file_location", this)
            {
                ShouldAllowEmpty = true,
                Description = "Location of XML file that will contain group-platform default values for site configuration.",
                DefaultValue = "DefaultSiteConfiguration.config"
            };
            DefaultMediaConfigurationFileLocation = new StringConfigurationValue("default_media_configuration_file_location", this)
            {
                ShouldAllowEmpty = true,
                Description = "Location of XML file that will contain group-platform default values for media configuration.",
                DefaultValue = "DefaultMediaConfiguration.config"
            };
        }
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
