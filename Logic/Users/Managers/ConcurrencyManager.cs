using ApiObjects;
using ApiObjects.MediaMarks;
using ApiObjects.Response;
using ApiObjects.Rules;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;
using TVinciShared;

namespace Core.Users
{
    public class ConcurrencyManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static DomainResponseStatus Validate(List<int> mediaRuleIds, List<long> assetRuleIds, Domain domain, int mediaId, string udid, int groupId, int deviceBrandId, int deviceFamilyId)
        {
            DomainResponseStatus status = DomainResponseStatus.UnKnown;

            if (mediaRuleIds != null && mediaRuleIds.Count > 0)
            {
                status = ValidateMediaConcurrency(mediaRuleIds, domain, mediaId, udid, groupId, deviceFamilyId);
            }

            if (assetRuleIds != null && assetRuleIds.Count > 0 &&
                (status == DomainResponseStatus.OK || status == DomainResponseStatus.UnKnown))
            {
                status = ValidateAssetRulesConcurrency(groupId, assetRuleIds, domain, udid, mediaId, deviceFamilyId);
            }

            // if it's MediaConcurrencyLimitation no need to check this one 
            if (status == DomainResponseStatus.OK || status == DomainResponseStatus.UnKnown)
            {
                status = ValidateDeviceFamilyConcurrency(udid, deviceBrandId, domain, groupId, deviceFamilyId);
            }

            return status;
        }

        /// <summary>
        /// This method get List of Media Concurrency Rules, Domain and MediaId
        /// Get from CB all media play at the last 
        /// </summary>
        /// <param name="mediaRuleIds"></param>
        /// <param name="domainId"></param>
        /// <param name="mediaId"></param>
        /// <param name="udid"></param>
        /// <returns></returns>
        private static DomainResponseStatus ValidateMediaConcurrency(List<int> mediaRuleIds, Domain domain, int mediaId, string udid, int groupId, int deviceFamilyId)
        {
            DomainResponseStatus response = DomainResponseStatus.OK;

            log.DebugFormat("ValidateAssetConcurrency, domainId:{0}, mediaId:{1}, ruleIds:{2}",
                            domain.m_nDomainID, mediaId, string.Join(",", mediaRuleIds));

            try
            {
                // Get all domain media marks
                List<DevicePlayData> devicePlayData = CatalogDAL.GetDomainPlayDataList(domain.m_nDomainID, new List<ePlayType>() { ePlayType.NPVR, ePlayType.MEDIA }, Utils.CONCURRENCY_MILLISEC_THRESHOLD);
                
                if (devicePlayData != null)
                {
                    // Get all group's media concurrency rules
                    var groupConcurrencyRules = Api.api.GetGroupMediaConcurrencyRules(groupId);

                    if (groupConcurrencyRules == null || groupConcurrencyRules.Count == 0)
                        return response;

                    ConcurrencyRestrictionPolicy policy = ConcurrencyRestrictionPolicy.Single;
                    int mediaConcurrencyLimit = 0;
                    List<DevicePlayData> assetDevicePlayData = null;

                    foreach (int ruleId in mediaRuleIds)
                    {
                        // Search for the relevant rules
                        var mediaConcurrencyRule = groupConcurrencyRules.FirstOrDefault(rule => rule.RuleID == ruleId);

                        // If we got one from the cache/api method, we will use its dat
                        if (mediaConcurrencyRule != null)
                        {
                            mediaConcurrencyLimit = mediaConcurrencyRule.Limitation;
                            policy = mediaConcurrencyRule.RestrictionPolicy;
                        }
                        // Otherwise we get the limiation from DB in the old fashion
                        else
                        {
                            // get limitation from DB
                            DataTable dt = ApiDAL.GetMCRulesByID(ruleId);
                            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                            {
                                mediaConcurrencyLimit = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "media_concurrency_limit");
                                policy = (ConcurrencyRestrictionPolicy)ODBCWrapper.Utils.ExtractInteger(dt.Rows[0], "restriction_policy");
                            }
                        }

                        if (mediaConcurrencyLimit == 0)
                            continue;

                        switch (policy)
                        {
                            case ConcurrencyRestrictionPolicy.Single:
                                {
                                    assetDevicePlayData = devicePlayData.FindAll(x => 
                                        !x.UDID.Equals(udid) && 
                                        x.AssetId == mediaId &&
                                        x.TimeStamp.UnixTimestampToDateTime().AddMilliseconds(Utils.CONCURRENCY_MILLISEC_THRESHOLD) > DateTime.UtcNow);
                                    break;
                                }
                            case ConcurrencyRestrictionPolicy.Group:
                                {
                                    assetDevicePlayData = devicePlayData.FindAll(x => 
                                        !x.UDID.Equals(udid) &&
                                        x.MediaConcurrencyRuleIds != null &&
                                        x.MediaConcurrencyRuleIds.Contains(ruleId) &&
                                        x.TimeStamp.UnixTimestampToDateTime().AddMilliseconds(Utils.CONCURRENCY_MILLISEC_THRESHOLD) > DateTime.UtcNow);
                                    break;
                                }
                            default:
                                break;
                        }

                        if (assetDevicePlayData != null && assetDevicePlayData.Count >= mediaConcurrencyLimit)
                        {
                            log.DebugFormat("MediaConcurrencyLimitation, domainId:{0}, mediaId:{1}, ruleId:{2}, limit:{3}, count:{4}",
                                domain.m_nDomainID, mediaId, ruleId, mediaConcurrencyLimit, assetDevicePlayData.Count);

                            return CheckDeviceConcurrencyPrioritization(groupId, udid, domain, deviceFamilyId, assetDevicePlayData.Select(x => x.DeviceFamilyId));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - ValidateMediaConcurrency", ex);
            }

            return response;
        }

        private static DomainResponseStatus ValidateAssetRulesConcurrency(int groupId, List<long> assetRuleIds, Domain domain, string udid, int mediaId, int deviceFamilyId)
        {
            if (assetRuleIds == null || assetRuleIds.Count == 0)
            {
                return DomainResponseStatus.OK;
            }

            try
            {
                List<DevicePlayData> devicePlayData = 
                    CatalogDAL.GetDomainPlayDataList(domain.m_nDomainID, new List<ePlayType>() { ePlayType.MEDIA }, Utils.CONCURRENCY_MILLISEC_THRESHOLD);

                if (devicePlayData == null || devicePlayData.Count == 0)
                {
                    return DomainResponseStatus.OK;
                }

                var groupAssetRules = Api.Module.GetAssetRules(AssetRuleConditionType.Concurrency, groupId);
                if (groupAssetRules == null || !groupAssetRules.HasObjects())
                    return DomainResponseStatus.OK;
                
                foreach (int currAssetRuleId in assetRuleIds)
                {
                    var currAssetRule = groupAssetRules.Objects.FirstOrDefault(x => x.Id == currAssetRuleId);
                    
                    if (currAssetRule == null)
                    {
                        continue;
                    }
                    
                    ConcurrencyCondition concurrencyCondition = currAssetRule.Conditions.FirstOrDefault(x => x is ConcurrencyCondition) as ConcurrencyCondition;
                    if (concurrencyCondition == null)
                    {
                        continue;
                    }

                    List<DevicePlayData> assetDevicePlayData = null;

                    switch (concurrencyCondition.RestrictionPolicy)
                    {
                        case ConcurrencyRestrictionPolicy.Single:
                            {
                                assetDevicePlayData = devicePlayData.FindAll
                                    (x => !x.UDID.Equals(udid) &&
                                     x.AssetId == mediaId &&
                                     x.TimeStamp.UnixTimestampToDateTime().AddMilliseconds(Utils.CONCURRENCY_MILLISEC_THRESHOLD) > DateTime.UtcNow);
                                break;
                            }
                        case ConcurrencyRestrictionPolicy.Group:
                            {
                                assetDevicePlayData = devicePlayData.FindAll
                                    (x => !x.UDID.Equals(udid) &&
                                     x.AssetConcurrencyRuleIds != null &&
                                     x.AssetConcurrencyRuleIds.Contains(currAssetRuleId) &&
                                     x.TimeStamp.UnixTimestampToDateTime().AddMilliseconds(Utils.CONCURRENCY_MILLISEC_THRESHOLD) > DateTime.UtcNow);
                                break;
                            }
                        default:
                            break;
                    }

                    if (assetDevicePlayData != null && assetDevicePlayData.Count >= concurrencyCondition.Limit)
                    {
                        log.DebugFormat("ValidateAssetRulesConcurrency, domainId:{0}, mediaId:{1}, assetRuleId:{2}, limit:{3}, count:{4}",
                                        domain, mediaId, currAssetRuleId, concurrencyCondition.Limit, assetDevicePlayData.Count);

                        return CheckDeviceConcurrencyPrioritization(groupId, udid, domain, deviceFamilyId, assetDevicePlayData.Select(x => x.DeviceFamilyId));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - ValidateAssetRulesConcurrency", ex);
                return DomainResponseStatus.Error;
            }

            return DomainResponseStatus.OK;
        }

        private static DomainResponseStatus ValidateDeviceFamilyConcurrency(string udid, int nDeviceBrandId, Domain domain, int groupId, int deviceFamilyId)
        {
            if (string.IsNullOrEmpty(udid))
            {
                return DomainResponseStatus.OK;
            }

            if (deviceFamilyId == 0)
            {
                return DomainResponseStatus.DeviceTypeNotAllowed;
            }

            if (domain.m_oLimitationsManager.Concurrency <= 0 || domain.IsAgnosticToDeviceLimitation(ValidationType.Concurrency, deviceFamilyId))
            {
                // there are no concurrency limitations at all.
                return DomainResponseStatus.OK;
            }

            int totalStreams = 0;
            // <familyId, streamingCount>
            Dictionary<int, int> deviceFamiliesStreams = domain.GetConcurrentCount(udid, ref totalStreams);
            if (deviceFamiliesStreams == null)
            {
                // no active streams at all
                return DomainResponseStatus.OK;
            }
            
            if (totalStreams >= domain.m_oLimitationsManager.Concurrency)
            {
                return CheckDeviceConcurrencyPrioritization(groupId, udid, domain, deviceFamilyId, deviceFamiliesStreams.Keys);
            }

            if (!deviceFamiliesStreams.ContainsKey(deviceFamilyId))
            {
                // no active streams at the device's family.
                return DomainResponseStatus.OK;
            }

            DeviceContainer deviceContainer = domain.GetDeviceContainerByUdid(udid);
            if (deviceContainer != null && 
                deviceFamiliesStreams[deviceContainer.m_deviceFamilyID] >= deviceContainer.m_oLimitationsManager.Concurrency)
            {
                // device family reached its max limit. Cannot allow a new stream
                return DomainResponseStatus.ConcurrencyLimitation;
            }

            // User is able to watch through this device. Hasn't reach the device family max limitation
            return DomainResponseStatus.OK;
        }

        private static DomainResponseStatus CheckDeviceConcurrencyPrioritization(int groupId, string udid, Domain domain, int deviceFamilyId, IEnumerable<int> otherDeviceFamilyIds)
        {
            DeviceConcurrencyPriority deviceConcurrencyPriority = Api.api.GetDeviceConcurrencyPriority(groupId);

            if (deviceConcurrencyPriority != null)
            {
                int currDevicePriorityIndex = deviceConcurrencyPriority.DeviceFamilyIds.IndexOf(deviceFamilyId);

                if (currDevicePriorityIndex != -1)
                {
                    foreach (var otherDeviceFamilyId in otherDeviceFamilyIds)
                    {
                        int otherDevicePriorityIndex = deviceConcurrencyPriority.DeviceFamilyIds.IndexOf(otherDeviceFamilyId);
                        if (otherDevicePriorityIndex == -1 || currDevicePriorityIndex < otherDevicePriorityIndex)
                        {
                            return DomainResponseStatus.OK;
                        }
                    }
                }
            }

            // Cannot allow a new stream. Domain reached its max limitation
            return DomainResponseStatus.ConcurrencyLimitation;
        }

        internal static Dictionary<string, int> GetDomainDevices(int domainId, int groupId)
        {
            Dictionary<string, int> domainDevices = CatalogDAL.GetDomainDevices(domainId);

            if (domainDevices == null)
            {
                DomainResponse domainResponse = Core.Domains.Module.GetDomainInfo(groupId, domainId);
                if (domainResponse.Status.Code == (int)eResponseStatus.OK && domainResponse.Domain != null)
                {
                    domainDevices = new Dictionary<string, int>();
                    foreach (var currDeviceFamily in domainResponse.Domain.m_deviceFamilies)
                    {
                        foreach (var currDevice in currDeviceFamily.DeviceInstances)
                        {
                            domainDevices.Add(currDevice.m_deviceUDID, currDeviceFamily.m_deviceFamilyID);
                        }
                    }

                    CatalogDAL.SaveDomainDevices(domainDevices, domainId);
                }
            }

            return domainDevices;
        }
    }
}
