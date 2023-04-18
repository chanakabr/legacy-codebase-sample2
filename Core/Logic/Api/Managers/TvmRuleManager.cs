using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Rules;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
using Core.Api;
using Core.Api.Managers;
using Core.Catalog.CatalogManagement;
using Core.Users;
using EpgBL;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;
using TVinciShared;

namespace APILogic.Api.Managers
{
    public class TvmRuleManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Public Methods

        internal static bool CheckGeoBlockMedia(Int32 groupId, Int32 mediaId, string ip, out string ruleName)
        {
            bool isBlocked = false;
            ruleName = "GeoAvailability";

            // check for Geo Restriction - White-list/Blacklist IPs    
            var rules = GetAssetRulesByIp(groupId, ip);

            if (rules != null && rules.Count > 0)
            {
                if (rules.Count(x => x.Actions != null && x.Actions.Any(a => a.Type == RuleActionType.BlockPlayback)) > 0)
                {
                    ruleName = "Blacklist";
                    return true;
                }
                else
                {
                    return isBlocked;
                }
            }

            Int32 geoBlockID = 0;
            var country = api.GetCountryByIp(groupId, ip);

            bool isGeoAvailability = false;
            int countryId = country != null ? country.Id : 0;
            isBlocked = api.IsMediaBlockedForCountryGeoAvailability(groupId, countryId, mediaId, out isGeoAvailability);

            if (!isGeoAvailability)
            {
                string key = LayeredCacheKeys.GetCheckGeoBlockMediaKey(groupId, mediaId);
                DataTable dt = null;
                // try to get from cache            
                bool cacheResult = LayeredCache.Instance.Get(key, 
                                                             ref dt, 
                                                             Get_GeoBlockPerMedia, 
                                                             new Dictionary<string, object>()
                                                             {
                                                                 { "groupId", groupId },
                                                                 { "mediaId", mediaId }
                                                             }, 
                                                             groupId, 
                                                             LayeredCacheConfigNames.CHECK_GEO_BLOCK_MEDIA_LAYERED_CACHE_CONFIG_NAME,
                                                             new List<string>() { LayeredCacheKeys.GetMediaInvalidationKey(groupId, mediaId) });
                if (cacheResult && dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    Int32 nCountryID = countryId;
                    ruleName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "NAME");
                    geoBlockID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "ID");
                    int nONLY_OR_BUT = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "ONLY_OR_BUT");

                    DataRow[] existingRows = dt.Select(string.Format("COUNTRY_ID={0}", nCountryID));
                    bool bExsitInRuleM2M = existingRows != null && existingRows.Length == 1 ? true : false;

                    log.Debug("Geo Blocks - Geo Block ID " + geoBlockID + " Country ID " + nCountryID);

                    if (geoBlockID > 0)
                    {
                        //No one except
                        if (nONLY_OR_BUT == 0)
                            isBlocked = !bExsitInRuleM2M;
                        //All except
                        if (nONLY_OR_BUT == 1)
                            isBlocked = bExsitInRuleM2M;

                        if (!isBlocked && ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "PROXY_RULE", 0) == 1) // then check what about the proxy - is it reliable 
                        {
                            isBlocked = (APILogic.Utils.IsProxyBlocked(groupId, ip));
                        }
                    }
                }
            }

            return isBlocked;
        }
        
        public static List<GroupRule> GetGroupMediaRules(int mediaId, string ip, string siteGuid, int groupId, string deviceUdid)
        {
            List<GroupRule> groupRules = new List<GroupRule>();

            //Check if geo-block applies
            string ruleName;
            if (CheckGeoBlockMedia(groupId, (int)mediaId, ip, out ruleName))
            {
                groupRules.Add(new GroupRule()
                {
                    Name = "GeoBlock",
                    IsActive = true,
                    BlockType = eBlockType.Geo,

                });
            }

            //Check if user type match media user types
            if (!string.IsNullOrEmpty(siteGuid) && api.CheckMediaUserType((int)mediaId, int.Parse(siteGuid), groupId) == false)
            {
                groupRules.Add(new GroupRule()
                {
                    RuleID = 0,
                    Name = "UserTypeBlock",
                    BlockType = eBlockType.UserType,
                    IsActive = true
                });
            }

            // check if the user have assetUserRules 
            long userId = !string.IsNullOrEmpty(siteGuid) ? long.Parse(siteGuid) : 0;
            var mediaAssetUserRules = AssetUserRuleManager.GetMediaAssetUserRulesToUser(groupId, userId, mediaId);

            if (mediaAssetUserRules != null && mediaAssetUserRules.Count > 0)
            {
                groupRules.AddRange(ConvertAssetUserRuleToGroupRule(mediaAssetUserRules));
            }

            // check if the user have Parental Rules 
            var response = api.GetParentalMediaRules(groupId, siteGuid, mediaId, 0);

            if (response != null && response.status != null && response.status.Code == 0)
            {
                groupRules.AddRange(api.ConvertParentalToGroupRule(response.rules));
            }

            return groupRules;

            #region old code
            //List<GroupRule> tempRules = getMediaUserRules(nMediaID, nSiteGuid);
            //List<GroupRule> resultRules = GetRules(eGroupRuleType.Parental, nSiteGuid, nMediaID, nGroupID, sIP, tempRules);
            //return resultRules; 
            #endregion
        }

        public static List<GroupRule> GetEPGProgramRules(int programId, int channelMediaId, string siteGuid, string ip, int groupId, string deviceUdid)
        {
            List<GroupRule> groupRules = new List<GroupRule>();

            //Check if geo-block applies
            string ruleName;
            if (string.IsNullOrEmpty(ip) || CheckGeoBlockMedia(groupId, (int)channelMediaId, ip, out ruleName))
            {
                groupRules.Add(new GroupRule()
                {
                    Name = "GeoBlock",
                    IsActive = true,
                    BlockType = eBlockType.Geo,

                });
            }

            //Check if user type match media user types
            if (!string.IsNullOrEmpty(siteGuid) && api.CheckMediaUserType((int)channelMediaId, int.Parse(siteGuid), groupId) == false)
            {
                groupRules.Add(new GroupRule()
                {
                    RuleID = 0,
                    Name = "UserTypeBlock",
                    BlockType = eBlockType.UserType,
                    IsActive = true
                });
            }

            var response = api.GetParentalEPGRules(groupId, siteGuid, programId, 0);

            if (response != null && response.status != null && response.status.Code == 0)
            {
                groupRules.AddRange(api.ConvertParentalToGroupRule(response.rules));
            }

            return groupRules;

            #region old code
            //List<GroupRule> tempRules = getEpgUserRules(nProgramId, nGroupId, nSiteGuid);
            //List<GroupRule> resultRules = GetRules(eGroupRuleType.EPG, nSiteGuid, nMediaId, nGroupId, sIP, tempRules);
            //return resultRules; 
            #endregion
        }

        public static List<GroupRule> GetNpvrRules(RecordedEPGChannelProgrammeObject recordedProgram, int siteGuid, string ip, int groupId, string deviceUdid)
        {
            int mediaGroupId = 0;
            int mediaId = 0;
            List<GroupRule> groupRules = new List<GroupRule>();

            // get media ID and non parent group ID
            DataSet mediaIdResultDataSet = Tvinci.Core.DAL.CatalogDAL.GetMediaByEpgChannelIds(groupId, new List<string>() { recordedProgram.EPG_CHANNEL_ID });

            if (mediaIdResultDataSet != null &&
                mediaIdResultDataSet.Tables != null &&
                mediaIdResultDataSet.Tables.Count > 0 &&
                mediaIdResultDataSet.Tables[0].Rows.Count > 0)
            {
                mediaGroupId = Utils.GetIntSafeVal(mediaIdResultDataSet.Tables[0].Rows[0], "GROUP_ID");
                mediaId = Utils.GetIntSafeVal(mediaIdResultDataSet.Tables[0].Rows[0], "id");

                if (mediaId != 0)
                {
                    // get EPG rules
                    List<GroupRule> tempRules = GetEpgUserRules(recordedProgram, groupId, siteGuid, mediaGroupId);

                    // combine user with EPG rules
                    groupRules = GetRules(eGroupRuleType.EPG, siteGuid, mediaId, groupId, ip, tempRules);
                }
            }

            if (mediaId == 0)
            {
                log.Debug(string.Format("GetEPGRecordedProgramRules - Failed to retrieve media ID using EPG ID. group ID: {0}, siteguid: {1}, EPG ID: {2}", 
                                        groupId, siteGuid, recordedProgram.EPG_IDENTIFIER));
            }

            return groupRules;
        }

        internal static Dictionary<long, TvmGeoRule> GetTvmGeoRulesFromCache(int groupId)
        {
            Dictionary<long, TvmGeoRule> result = null;

            try
            {
                string key = LayeredCacheKeys.GetGroupGeoBlockRulesKey(groupId);
                if (!LayeredCache.Instance.Get(key,
                                               ref result,
                                               GetGroupGeoblockRules,
                                               new Dictionary<string, object>() { { "groupId", groupId } },
                                               groupId,
                                               LayeredCacheConfigNames.GET_GROUP_GEO_BLOCK_RULES_CACHE_CONFIG_NAME,
                                               new List<string>() { LayeredCacheKeys.GetGroupGeoBlockRulesInvalidationKey(groupId) }))
                {
                    log.ErrorFormat("Failed getting GetGroupGeoBlockRules from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetTvmGeoRuleFromCache for groupId: {0}", groupId), ex);
            }

            return result;
        }

        internal static bool ValidateGeoBlockRuleExists(int groupId, int geoBlockRuleId)
        {
            bool res = false;
            var geoblockRules = GetTvmGeoRulesFromCache(groupId);
            if (geoblockRules != null)
            {
                res = geoblockRules.ContainsKey(geoBlockRuleId);
            }

            return res;
        }

        internal static string GetGeoBlockRuleName(int groupId, int geoblockRuleId)
        {
            var geoblockRules = GetTvmGeoRulesFromCache(groupId);
            if (geoblockRules == null || geoblockRules.Count == 0)
            {
                log.ErrorFormat("group geoblockRules were not found. groupId: {0}", groupId);
                return string.Empty;
            }

            if (geoblockRules.ContainsKey(geoblockRuleId))
                return geoblockRules[geoblockRuleId].Name;
            else
            {
                log.ErrorFormat("group geoblockRule {0} was not found. groupId: {1}", geoblockRuleId, groupId);
                return string.Empty;
            }
        }

        internal static long? GetGeoBlockRuleId(int groupId, string geoBlockRuleName)
        {
            if (geoBlockRuleName.IsNullOrEmptyOrWhiteSpace())
            {
                return null;
            }

            var geoblockRules = GetTvmGeoRulesFromCache(groupId);
            if (geoblockRules == null || geoblockRules.Count == 0)
            {
                log.ErrorFormat("group geoblockRules were not found. groupId: {0}", groupId);
                return null;
            }

            var geoblockRule = geoblockRules.FirstOrDefault(x => x.Value.Name.ToLower().Equals(geoBlockRuleName.ToLower()));
            if (!geoblockRule.IsDefault())
            {
                return geoblockRule.Key;
            }

            log.ErrorFormat("group geoblockRule {0} was not found. groupId: {1}", geoBlockRuleName, groupId);
            return null;
        }

        public static GenericListResponse<TvmRule> GetTvmRules(int groupId, TvmRuleType? ruleTypeEqual, string nameEqual)
        {
            GenericListResponse<TvmRule> tvmRules = new GenericListResponse<TvmRule>();

            try
            {
                // get geo rules
                if (!ruleTypeEqual.HasValue || ruleTypeEqual.Value == TvmRuleType.Geo)
                {
                    var tvmGeoRules = GetTvmGeoRulesFromCache(groupId);
                    if (tvmGeoRules != null && tvmGeoRules.Count > 0)
                    {
                        if (string.IsNullOrEmpty(nameEqual))
                        {
                            tvmRules.Objects.AddRange(tvmGeoRules.Values);
                        }
                        else
                        {
                            var geoRule = tvmGeoRules.FirstOrDefault(x => x.Value.Name.ToLower().Equals(nameEqual.ToLower()));
                            if (!geoRule.IsDefault())
                            {
                                tvmRules.Objects.Add(geoRule.Value);
                            }
                        }
                    }
                }

                // get device rules
                if (!ruleTypeEqual.HasValue || ruleTypeEqual.Value == TvmRuleType.Device)
                {
                    var tvmDeviceRules = GetTvmDeviceRulesFromCache(groupId);
                    if (tvmDeviceRules != null && tvmDeviceRules.Count > 0)
                    {
                        if (string.IsNullOrEmpty(nameEqual))
                        {
                            tvmRules.Objects.AddRange(tvmDeviceRules.Values);
                        }
                        else
                        {
                            var deviceRule = tvmDeviceRules.FirstOrDefault(x => x.Value.Name.ToLower().Equals(nameEqual.ToLower()));
                            if (!deviceRule.IsDefault())
                            {
                                tvmRules.Objects.Add(deviceRule.Value);
                            }
                        }
                    }
                }

                tvmRules.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                tvmRules.SetStatus(eResponseStatus.Error);
                log.Error(string.Format("Failed GetTvmRules groupId:{0}, ruleTypeEqual:{1}, nameEqual:{2}.", groupId, ruleTypeEqual, nameEqual), ex);
            }

            return tvmRules;
        }

        internal static Dictionary<long, TvmDeviceRule> GetTvmDeviceRulesFromCache(int groupId)
        {
            Dictionary<long, TvmDeviceRule> result = null;
            try
            {
                string key = LayeredCacheKeys.GetGroupDeviceRulesKey(groupId);
                if (!LayeredCache.Instance.Get(key, 
                                               ref result, 
                                               GetGroupDeviceRules, 
                                               new Dictionary<string, object>() { { "groupId", groupId } }, 
                                               groupId,
                                               LayeredCacheConfigNames.GET_GROUP_DEVICE_RULES_CACHE_CONFIG_NAME, 
                                               new List<string>() { LayeredCacheKeys.GetGroupDeviceRulesInvalidationKey(groupId) }))
                {
                    log.ErrorFormat("Failed getting GetGroupDeviceRules from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetTvmDeviceRulesFromCache for groupId: {0}", groupId), ex);
            }

            return result;
        }

        internal static bool ValidateDeviceRuleExists(int groupId, long deviceRuleId)
        {
            bool res = false;
            var deviceRules = GetTvmDeviceRulesFromCache(groupId);
            if (deviceRules != null)
            {
                res = deviceRules.ContainsKey(deviceRuleId);
            }

            return res;
        }

        internal static string GetDeviceRuleName(int groupId, long deviceRuleId)
        {
            var deviceRules = GetTvmDeviceRulesFromCache(groupId);
            if (deviceRules == null || deviceRules.Count == 0)
            {
                log.ErrorFormat("group deviceRules were not found. groupId: {0}", groupId);
                return string.Empty;
            }

            if (deviceRules.ContainsKey(deviceRuleId))
                return deviceRules[deviceRuleId].Name;
            else
            {
                log.ErrorFormat("group deviceRule {0} was not found. groupId: {1}", deviceRuleId, groupId);
                return string.Empty;
            }
        }

        internal static long? GetDeviceRuleId(int groupId, string deviceRuleName)
        {
            if (deviceRuleName.IsNullOrEmptyOrWhiteSpace())
            {
                return null;
            }

            var deviceRules = GetTvmDeviceRulesFromCache(groupId);
            if (deviceRules == null || deviceRules.Count == 0)
            {
                log.ErrorFormat("group deviceRules were not found. groupId: {0}", groupId);
                return null;
            }

            var deviceRule = deviceRules.FirstOrDefault(x => x.Value.Name.ToLower().Equals(deviceRuleName.ToLower()));

            if (!deviceRule.IsDefault())
            {
                return deviceRule.Key;
            }
            
            log.ErrorFormat("group deviceRule {0} was not found. groupId: {1}", deviceRuleName, groupId);
            return null;
        }

        #endregion

        #region Private Methods 

        private static Tuple<Dictionary<long, TvmDeviceRule>, bool> GetGroupDeviceRules(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<long, TvmDeviceRule> result = null;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue && groupId.Value > 0)
                    {
                        DataTable dt = CatalogDAL.GetGroupDeviceRules(groupId.Value);
                        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                        {
                            result = new Dictionary<long, TvmDeviceRule>();
                            foreach (DataRow dr in dt.Rows)
                            {
                                var deviceRule = new TvmDeviceRule()
                                {
                                    Id = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID", 0),
                                    GroupId = groupId.Value,
                                    Name = ODBCWrapper.Utils.GetSafeStr(dr, "NAME"),
                                    CreateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE")),
                                    DeviceBrandIds = new HashSet<int>()
                                };

                                var deviceBrandId = ODBCWrapper.Utils.GetIntSafeVal(dr, "BRAND_ID");

                                if (!result.ContainsKey(deviceRule.Id))
                                {
                                    if (deviceBrandId > 0)
                                    {
                                        deviceRule.DeviceBrandIds.Add(deviceBrandId);
                                    }

                                    result.Add(deviceRule.Id, deviceRule);
                                }
                                else if (deviceBrandId > 0 && !result[deviceRule.Id].DeviceBrandIds.Contains(deviceBrandId))
                                {
                                    result[deviceRule.Id].DeviceBrandIds.Add(deviceBrandId);
                                }
                            }
                        }
                    }
                }

                res = result != null;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetGroupDeviceRules failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<long, TvmDeviceRule>, bool>(result, res);
        }

        private static Tuple<DataTable, bool> Get_GeoBlockPerMedia(Dictionary<string, object> funcParams)
        {
            var res = false;
            DataTable dt = null;
            try
            {
                if (funcParams != null && funcParams.Count == 2 && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("mediaId"))
                {
                    int? groupId, mediaId;
                    groupId = funcParams["groupId"] as int?;
                    mediaId = funcParams["mediaId"] as int?;
                    if (groupId.HasValue && mediaId.HasValue)
                    {
                        dt = DAL.ApiDAL.Get_GeoBlockRuleForMediaAndCountries(groupId.Value, mediaId.Value);
                        res = dt != null && dt.Rows != null;
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Get_GeoBlockPerMedia failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<DataTable, bool>(dt, res);
        }

        private static Tuple<Dictionary<long, TvmGeoRule>, bool> GetGroupGeoblockRules(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<long, TvmGeoRule> result = null;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue && groupId.Value > 0)
                    {
                        DataTable dt = CatalogDAL.GetGroupGeoblockRules(groupId.Value);
                        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                        {
                            result = new Dictionary<long, TvmGeoRule>();
                            foreach (DataRow dr in dt.Rows)
                            {
                                var geoRule = new TvmGeoRule()
                                {
                                    Id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0),
                                    GroupId = groupId.Value,
                                    Name = ODBCWrapper.Utils.GetSafeStr(dr, "NAME"),
                                    CreateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE")),
                                    OnlyOrBut = ODBCWrapper.Utils.GetIntSafeVal(dr, "ONLY_OR_BUT") == 0 ? false : true,
                                    ProxyRuleId = ODBCWrapper.Utils.GetIntSafeVal(dr, "PROXY_RULE"),
                                    ProxyRuleName = ODBCWrapper.Utils.GetSafeStr(dr, "PROXY_RULE_NAME"),
                                    ProxyLevelId = ODBCWrapper.Utils.GetIntSafeVal(dr, "PROXY_LEVEL"),
                                    ProxyLevelName = ODBCWrapper.Utils.GetSafeStr(dr, "PROXY_LEVEL_NAME"),
                                    CountryIds = new HashSet<int>()
                                };

                                var countryId = ODBCWrapper.Utils.GetIntSafeVal(dr, "COUNTRY_ID");

                                if (!result.ContainsKey(geoRule.Id))
                                {
                                    if (countryId > 0)
                                    {
                                        geoRule.CountryIds.Add(countryId);
                                    }

                                    result.Add(geoRule.Id, geoRule);
                                }
                                else if (countryId > 0 && !result[geoRule.Id].CountryIds.Contains(countryId))
                                {
                                    result[geoRule.Id].CountryIds.Add(countryId);
                                }
                            }
                        }
                    }
                }

                res = result != null;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetGroupGeoblockRules failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<long, TvmGeoRule>, bool>(result, res);
        }

        private static List<AssetRule> GetAssetRulesByIp(int groupId, string ip)
        {
            List<AssetRule> assetRules = null;

            // convert ip address to number
            if (!APILogic.Utils.ConvertIpToNumber(ip, out var convertedIp, out var isV6)
                || !isV6 && long.Parse(convertedIp) == 0
                || isV6 && string.IsNullOrEmpty(convertedIp))
            {
                //invalid ip
                return assetRules;
            }

            // check if assetRule exist
            GenericListResponse<AssetRule> assetRulesResponse = AssetRuleManager.Instance.GetAssetRules(RuleConditionType.IP_RANGE, groupId);
            if (assetRulesResponse == null || !assetRulesResponse.HasObjects())
                return assetRules;

            assetRules = assetRulesResponse.Objects.Where(x => x.Conditions.Any(z => z.Type == RuleConditionType.IP_RANGE) &&
                         x.Conditions.OfType<IpRangeCondition>().Any(p => p.IpFrom <= long.Parse(convertedIp) && long.Parse(convertedIp) <= p.IpTo)).ToList();

            var ipV6Rules = assetRulesResponse.Objects.Where(x => x.Conditions.Any(z => z.Type == RuleConditionType.IP_V6_RANGE) &&
                                                               x.Conditions.OfType<IpV6RangeCondition>().Any(p => new IPAddressRange().Init(p.FromIp, p.ToIp).IsInRange(ip))).ToList();
            assetRules.AddRange(ipV6Rules);
            return assetRules;
        }
        
        private static List<GroupRule> ConvertAssetUserRuleToGroupRule(List<AssetUserRule> assetUserRules)
        {
            List<GroupRule> groupRules = new List<GroupRule>();

            foreach (var assetUserRule in assetUserRules)
            {
                GroupRule groupRule = new GroupRule()
                {
                    RuleID = (int)assetUserRule.Id,
                    IsActive = true,
                    Name = assetUserRule.Name,
                    GroupRuleType = eGroupRuleType.AssetUser,
                    BlockType = eBlockType.AssetUserBlock
                };

                groupRules.Add(groupRule);
            }

            return groupRules;
        }

        private static List<GroupRule> GetRules(eGroupRuleType eRuleType, int nSiteGuid, int nMediaId, int nGroupID, string sIP, List<GroupRule> tempRules)
        {
            List<GroupRule> rules = new List<GroupRule>();

            //Check if geo-block applies
            string ruleName;
            if (CheckGeoBlockMedia(nGroupID, nMediaId, sIP, out ruleName))
            {
                rules.Add(new GroupRule() { Name = "GeoBlock", BlockType = eBlockType.Geo });
            }

            //Check if user type match media user types
            if (nSiteGuid > 0 && api.CheckMediaUserType(nMediaId, nSiteGuid, nGroupID) == false)
            {
                rules.Add(new GroupRule() { Name = "UserTypeBlock", BlockType = eBlockType.UserType });
            }

            if (tempRules != null && tempRules.Count > 0)
            {
                foreach (GroupRule rule in tempRules)
                {
                    if (rule.GroupRuleType == eRuleType && rule.BlockType != eBlockType.Geo)
                    {
                        if (nSiteGuid > 0)
                        {
                            if (rule.AgeRestriction > 0 && !CheckAgeValidation(rule.AgeRestriction, nSiteGuid)) //check for active????
                            {
                                rule.BlockType = eBlockType.AgeBlock;
                                rules.Add(rule);
                            }
                            else
                            {
                                if (rule.IsActive)
                                {
                                    rule.BlockType = eBlockType.Validation;
                                    rules.Add(rule);
                                }
                            }
                        }
                        else //check for anonymous Rules
                        {
                            if (rule.BlockAnonymous && rule.IsActive)
                            {
                                rule.BlockType = eBlockType.AnonymousAccessBlock;
                                rules.Add(rule);
                            }
                        }
                    }
                }
            }

            return rules;
        }
        
        /// <summary>
        /// get All epg rules that are relevant for the user
        /// </summary>
        /// <param name="nSiteGuid"></param>
        /// <param name="nGroupID"></param>
        /// <returns></returns>
        private static List<GroupRule> GetAllEpgUserRules(int nSiteGuid, int nGroupID)
        {
            DataTable rulesDt = DAL.ApiDAL.Get_EPGRules(nSiteGuid.ToString(), nGroupID);
            List<GroupRule> userRules = new List<GroupRule>();
            GroupRule rule = new GroupRule();
            if (rulesDt != null)
            {
                if (rulesDt.Rows != null && rulesDt.Rows.Count > 0)
                {
                    for (int i = 0; i < rulesDt.Rows.Count; i++)
                    {
                        int nRuleID = APILogic.Utils.GetIntSafeVal(rulesDt.Rows[i], "rule_id");
                        int nTagTypeID = APILogic.Utils.GetIntSafeVal(rulesDt.Rows[i], "TAG_TYPE_ID");
                        string sValue = APILogic.Utils.GetSafeStr(rulesDt.Rows[i], "VALUE");
                        string sKey = APILogic.Utils.GetSafeStr(rulesDt.Rows[i], "Key");
                        string sName = APILogic.Utils.GetSafeStr(rulesDt.Rows[i], "Name");
                        object sAgeRestriction = rulesDt.Rows[i]["age_restriction"];
                        int nIsActive = APILogic.Utils.GetIntSafeVal(rulesDt.Rows[i], "is_active");
                        eGroupRuleType eRuleType = (eGroupRuleType)Utils.GetIntSafeVal(rulesDt.Rows[i], "group_rule_type_id");
                        int nBlockAnonymous = APILogic.Utils.GetIntSafeVal(rulesDt.Rows[i], "block_anonymous");
                        bool bBlockAnonymous = (nBlockAnonymous == 1);
                        string tagType = APILogic.Utils.GetSafeStr(rulesDt.Rows[i], "tagName");
                        rule = new GroupRule(nRuleID, nTagTypeID, sValue, sKey, sName, sAgeRestriction, nIsActive, eRuleType, bBlockAnonymous, tagType);
                        userRules.Add(rule);
                    }
                }
            }
            return userRules;
        }

        /// <summary>
        /// get the EPG rules that are relevant for the user and for the program
        /// </summary>
        /// <param name="recordedProgram"></param>
        /// <param name="groupId"></param>
        /// <param name="siteGuid"></param>
        /// <param name="nonParentGroupId"></param>
        /// <returns></returns>
        private static List<GroupRule> GetEpgUserRules(RecordedEPGChannelProgrammeObject recordedProgram, int groupId, int siteGuid, int nonParentGroupId)
        {
            List<GroupRule> epgRules = new List<GroupRule>();
            int parentGroupId = DAL.UtilsDal.GetParentGroupID(groupId);
            List<GroupRule> userRules = GetAllEpgUserRules(siteGuid, nonParentGroupId);

            //add only rules for tags that are tags of the specific EPG           
            foreach (GroupRule rule in userRules)
            {
                if (rule != null)
                {
                    var ruleTag = recordedProgram.EPG_TAGS.Where(x => x.Key.ToLower() == rule.TagType.ToLower());
                    foreach (EPGDictionary epgItem in ruleTag)
                    {
                        if (!string.IsNullOrEmpty(epgItem.Key))
                        {
                            if (epgItem.Value.ToLower() == rule.TagValue.ToLower())
                                epgRules.Add(rule);
                        }
                    }
                }
            }
            return epgRules;
        }

        private static bool CheckAgeValidation(int ageLimit, int userID)
        {
            bool retVal = false;
            DataTable dt = DAL.ApiDAL.Get_DetailsUsersDynamicData(userID);
            if (dt != null)
            {
                if (dt.DefaultView.Count > 0)
                {
                    int count = dt.DefaultView.Count;
                    int birthDay = 0;
                    int birthMonth = 0;
                    int birthYear = 0;
                    DateTime birthDate = DateTime.MaxValue;
                    for (int i = 0; i < count; i++)
                    {
                        string dataType = dt.Rows[i]["data_type"].ToString();
                        string dataValue = dt.Rows[i]["data_value"].ToString();
                        if (dataType.ToLower().Equals("birthday"))
                        {
                            if (dataValue.ToLower().Contains("/"))
                            {
                                if (DateTime.TryParse(dataValue.ToLower(), out birthDate))
                                {
                                    break;
                                }
                            }
                            else
                            {
                                birthDay = int.Parse(dataValue);
                            }
                        }
                        else if (dataType.ToLower().Equals("birthmonth"))
                        {
                            birthMonth = int.Parse(dataValue);
                        }
                        else if (dataType.ToLower().Equals("birthyear"))
                        {
                            birthYear = int.Parse(dataValue);
                        }

                    }
                    if (birthDate == DateTime.MaxValue)
                    {
                        if (birthYear != 0 && birthYear != 0)
                        {
                            birthDate = new DateTime(birthYear, birthMonth, birthDay);
                        }
                    }
                    if (birthDate != DateTime.MaxValue)
                    {
                        if (birthDate.CompareTo(DateTime.UtcNow.AddYears(-ageLimit)) < 0)
                        {
                            retVal = true;
                        }
                    }
                }
            }

            return retVal;
        }
        
        #endregion
    }
}
