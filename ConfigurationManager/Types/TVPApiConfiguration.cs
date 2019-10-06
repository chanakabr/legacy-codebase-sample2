using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ConfigurationManager
{
    public class TVPApiConfiguration : ConfigurationValue
    {
        public NumericConfigurationValue CacheLiteDurationInMinutes;
        //public StringConfigurationValue MediaConfigurationVirtualPath;
        //public StringConfigurationValue OrcaRecommendationsConfigurationVirtualPath;
        public BooleanConfigurationValue ShouldUseNewCache;
        public OfflineFavoriteSyncGroupsConfiguration OfflineFavoriteSyncGroups;
        public NumericConfigurationValue OdbcCacheSeconds;
        public NumericConfigurationValue AuthorizationGroupConfigsTtlSeconds;
        public CommaSeparatedConfigurationValue AuthorizationUnsupportedGroupsPlatforms;
        public StringConfigurationValue MainConnectionString;

        public TVPApiConfiguration(string key) : base(key)
        {
            CacheLiteDurationInMinutes = new NumericConfigurationValue("cache_lite_duration_in_minutes", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = 1440
            };
            //MediaConfigurationVirtualPath = new StringConfigurationValue("media_configuration_virtual_path", this)
            //{
            //    ShouldAllowEmpty = true
            //};
            //OrcaRecommendationsConfigurationVirtualPath = new StringConfigurationValue("orca_recommendation_configuration_virtual_path", this)
            //{
            //    ShouldAllowEmpty = true
            //};
            ShouldUseNewCache = new BooleanConfigurationValue("should_use_new_cache", this)
            {
                ShouldAllowEmpty = true,
                DefaultValue = true
            };
            OfflineFavoriteSyncGroups = new OfflineFavoriteSyncGroupsConfiguration("offline_favorite_sync_groups", this)
            {
                ShouldAllowEmpty = true,
                Description = "Comma separted list of groups that should use offline favorite sync"
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
                ShouldAllowEmpty = true
            };
            MainConnectionString = new StringConfigurationValue("main_connection_string", this)
            {
                ShouldAllowEmpty = true
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
