using System.Collections.Generic;
using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;

namespace ConfigurationManager
{
    public class TVPApiConfiguration : BaseConfig<TVPApiConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.TVPApiConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        private static readonly HashSet<string> authorizationUnsupportedGroupsPlatformsSet = new HashSet<string>();
        private static readonly HashSet<int> offlineFavoriteSyncGroupsSet = new HashSet<int>();
        private const string AuthrizationUnsupportedGroupPlatforsmKey = "authorization_unsupported_groups_platforms";
        private const string Offline_favorite_sync_groupsKey = "offline_favorite_sync_groups";

        public BaseValue<HashSet<string>> AuthorizationUnsupportedGroupsPlatforms = new BaseValue<HashSet<string>>(AuthrizationUnsupportedGroupPlatforsmKey, authorizationUnsupportedGroupsPlatformsSet);
        public BaseValue<HashSet<int>> OfflineFavoriteSyncGroups = new BaseValue<HashSet<int>>(Offline_favorite_sync_groupsKey, offlineFavoriteSyncGroupsSet);


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


        public override void SetActualValue<TV>(JToken token, BaseValue<TV> defaultData)
        {
            if (defaultData.Key == AuthrizationUnsupportedGroupPlatforsmKey)
            {
                PopulateList(token, defaultData as BaseValue<HashSet<string>>);
            }
            else if(defaultData.Key == Offline_favorite_sync_groupsKey)
            {
                PopulateList(token, defaultData as BaseValue<HashSet<int>>);
            }
            else
            {
                base.SetActualValue(token, defaultData);
            }
        }

        private void PopulateList(JToken token, BaseValue<HashSet<int>> defaultData)
        {
            HashSet<int> res = null;
            if (token != null)
            {
                string[] values = token.ToString().Split(',');
                res = new HashSet<int>();
                foreach (var value in values)
                {
                    int groupId = 0;

                    if (!int.TryParse(value, out groupId))
                    {
                        _Logger.Error($"Group Ids should be comma separated list of numbers. Found invalid value {value}");
                    }
                    else
                    {
                        res.Add(groupId);
                    }
                }
            }
            defaultData.ActualValue = res;
        }

        private void PopulateList(JToken token, BaseValue<HashSet<string>> defaultData) 
        {
            var res  = new HashSet<string>();

            if (token != null)
            {
                string[] splitted = token.ToString().Split(',');

                foreach (var value in splitted)
                {
                    res.Add(value);
                }
            }
            defaultData.ActualValue = res;
        }
    }

}
