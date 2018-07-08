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
       
        public static DomainResponseStatus Validate(DevicePlayData devicePlayData, Domain domain, int groupId, int deviceFamilyId)
        {
            DomainResponseStatus status = DomainResponseStatus.UnKnown;

            if (devicePlayData.MediaConcurrencyRuleIds != null && devicePlayData.MediaConcurrencyRuleIds.Count > 0)
            {
                status = ValidateMediaConcurrency(devicePlayData.MediaConcurrencyRuleIds, domain, devicePlayData.AssetId, devicePlayData.UDID, groupId, deviceFamilyId);
            }

            if (devicePlayData.AssetMediaConcurrencyRuleIds != null && devicePlayData.AssetMediaConcurrencyRuleIds.Count > 0 &&
                (status == DomainResponseStatus.OK || status == DomainResponseStatus.UnKnown))
            {
                status = ValidateAssetRulesConcurrency(groupId, devicePlayData.AssetMediaConcurrencyRuleIds, domain, devicePlayData.UDID, devicePlayData.AssetId, deviceFamilyId, eAssetTypes.MEDIA);
            }

            if (devicePlayData.AssetEpgConcurrencyRuleIds != null && devicePlayData.AssetEpgConcurrencyRuleIds.Count > 0 &&
                (status == DomainResponseStatus.OK || status == DomainResponseStatus.UnKnown))
            {
                status = ValidateAssetRulesConcurrency(groupId, devicePlayData.AssetEpgConcurrencyRuleIds, domain, devicePlayData.UDID, devicePlayData.ProgramId, deviceFamilyId, eAssetTypes.EPG);
            }

            // if it's MediaConcurrencyLimitation no need to check this one 
            if (status == DomainResponseStatus.OK || status == DomainResponseStatus.UnKnown)
            {
                status = ValidateDeviceFamilyConcurrency(devicePlayData.UDID, domain, groupId, deviceFamilyId);
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
            if (mediaRuleIds == null || mediaRuleIds.Count == 0)
            {
                return DomainResponseStatus.OK;
            }

            log.DebugFormat("ValidateAssetConcurrency, domainId:{0}, mediaId:{1}, ruleIds:{2}",
                            domain.m_nDomainID, mediaId, string.Join(",", mediaRuleIds));

            try
            {
                // Get all domain media marks
                List<DevicePlayData> devicePlayData = CatalogDAL.GetDomainPlayDataList(GetDomainDevices(domain.m_nDomainID, groupId),
                                                                                       new List<ePlayType>() { ePlayType.NPVR, ePlayType.MEDIA }, 
                                                                                       Utils.CONCURRENCY_MILLISEC_THRESHOLD);

                if (devicePlayData == null || devicePlayData.Count == 0)
                {
                    return DomainResponseStatus.OK;
                }

                // Get all group's media concurrency rules
                var groupConcurrencyRules = Api.api.GetGroupMediaConcurrencyRules(groupId);

                if (groupConcurrencyRules == null || groupConcurrencyRules.Count == 0)
                {
                    return DomainResponseStatus.OK;
                }
                
                foreach (int ruleId in mediaRuleIds)
                {
                    // Search for the relevant rules
                    var mediaConcurrencyRule = groupConcurrencyRules.FirstOrDefault(rule => rule.RuleID == ruleId);

                    int mediaConcurrencyLimit = 0;
                    ConcurrencyRestrictionPolicy policy = ConcurrencyRestrictionPolicy.Single;

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

                    List<DevicePlayData> assetDevicePlayData = null;

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

                        if (CheckDeviceConcurrencyPrioritization(groupId, udid, domain, deviceFamilyId, assetDevicePlayData.Select(x => x.DeviceFamilyId)) == DomainResponseStatus.ConcurrencyLimitation)
                            return DomainResponseStatus.MediaConcurrencyLimitation;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - ValidateMediaConcurrency", ex);
                return DomainResponseStatus.Error;
            }

            return DomainResponseStatus.OK;
        }

        private static DomainResponseStatus ValidateAssetRulesConcurrency(int groupId, List<long> assetRuleIds, Domain domain, string udid, 
                                                                          long assetId, int deviceFamilyId, eAssetTypes assetType)
        {
            if (assetRuleIds == null || assetRuleIds.Count == 0)
            {
                return DomainResponseStatus.OK;
            }

            try
            {
                List<ePlayType> playTypes = new List<ePlayType>() { ePlayType.MEDIA };
                if (assetType == eAssetTypes.EPG)
                {
                    playTypes.Add(ePlayType.EPG);
                }

                List<DevicePlayData> devicePlayData = 
                    CatalogDAL.GetDomainPlayDataList(GetDomainDevices(domain.m_nDomainID, groupId), playTypes, Utils.CONCURRENCY_MILLISEC_THRESHOLD);

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
                    if (concurrencyCondition == null || concurrencyCondition.Limit == 0)
                    {
                        continue;
                    }

                    List<DevicePlayData> assetDevicePlayData = null;
                    if (assetType == eAssetTypes.MEDIA)
                    {
                        assetDevicePlayData = GetAssetMediaDevicePlayData(concurrencyCondition.RestrictionPolicy, devicePlayData, udid, assetId, currAssetRuleId);
                    }
                    else if (assetType == eAssetTypes.EPG)
                    {
                        assetDevicePlayData = GetAssetEpgDevicePlayData(concurrencyCondition.RestrictionPolicy, devicePlayData, udid, assetId, currAssetRuleId);
                    }
                     

                    if (assetDevicePlayData != null && assetDevicePlayData.Count >= concurrencyCondition.Limit)
                    {
                        log.DebugFormat("ValidateAssetRulesConcurrency, domainId:{0}, mediaId:{1}, assetRuleId:{2}, limit:{3}, count:{4}",
                                        domain, assetId, currAssetRuleId, concurrencyCondition.Limit, assetDevicePlayData.Count);

                        if (CheckDeviceConcurrencyPrioritization(groupId, udid, domain, deviceFamilyId, assetDevicePlayData.Select(x => x.DeviceFamilyId)) == DomainResponseStatus.ConcurrencyLimitation)
                            return DomainResponseStatus.MediaConcurrencyLimitation;
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

        private static List<DevicePlayData> GetAssetMediaDevicePlayData(ConcurrencyRestrictionPolicy restrictionPolicy, List<DevicePlayData> devicePlayData, 
                                                                        string udid, long mediaId, int assetRuleId)
        {
            List<DevicePlayData> assetDevicePlayData = null;

            switch (restrictionPolicy)
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
                             x.AssetMediaConcurrencyRuleIds != null &&
                             x.AssetMediaConcurrencyRuleIds.Contains(assetRuleId) &&
                             x.TimeStamp.UnixTimestampToDateTime().AddMilliseconds(Utils.CONCURRENCY_MILLISEC_THRESHOLD) > DateTime.UtcNow);
                        break;
                    }
                default:
                    break;
            }

            return assetDevicePlayData;
        }

        private static List<DevicePlayData> GetAssetEpgDevicePlayData(ConcurrencyRestrictionPolicy restrictionPolicy, List<DevicePlayData> devicePlayData,
                                                                        string udid, long programId, int assetRuleId)
        {
            List<DevicePlayData> assetDevicePlayData = null;

            switch (restrictionPolicy)
            {
                case ConcurrencyRestrictionPolicy.Single:
                    {
                        assetDevicePlayData = devicePlayData.FindAll
                            (x => !x.UDID.Equals(udid) &&
                             x.ProgramId == programId &&
                             x.TimeStamp.UnixTimestampToDateTime().AddMilliseconds(Utils.CONCURRENCY_MILLISEC_THRESHOLD) > DateTime.UtcNow);
                        break;
                    }
                case ConcurrencyRestrictionPolicy.Group:
                    {
                        assetDevicePlayData = devicePlayData.FindAll
                            (x => !x.UDID.Equals(udid) &&
                             x.AssetEpgConcurrencyRuleIds != null &&
                             x.AssetEpgConcurrencyRuleIds.Contains(assetRuleId) &&
                             x.TimeStamp.UnixTimestampToDateTime().AddMilliseconds(Utils.CONCURRENCY_MILLISEC_THRESHOLD) > DateTime.UtcNow);
                        break;
                    }
                default:
                    break;
            }

            return assetDevicePlayData;
        }

        private static DomainResponseStatus ValidateDeviceFamilyConcurrency(string udid, Domain domain, int groupId, int deviceFamilyId)
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

        private static DomainResponseStatus CheckDeviceConcurrencyPrioritization(int groupId, string udid, Domain domain, int currDeviceFamilyId, IEnumerable<int> otherDeviceFamilyIds)
        {
            DeviceConcurrencyPriority deviceConcurrencyPriority = Api.api.GetDeviceConcurrencyPriority(groupId);

            if (deviceConcurrencyPriority != null)
            {
                int currDevicePriorityIndex = deviceConcurrencyPriority.DeviceFamilyIds.IndexOf(currDeviceFamilyId);

                if (currDevicePriorityIndex != -1)
                {
                    if (deviceConcurrencyPriority.PriorityOrder == DowngradePolicy.LIFO)
                    {
                        otherDeviceFamilyIds = otherDeviceFamilyIds.Reverse();
                    }

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

        internal static int GetDeviceFamilyIdByUdid(int domainId, int groupId, string udid)
        {
            int deviceFamilyId = 0;
            Dictionary<string, int> domainDevices = GetDomainDevices(domainId, groupId);
            
            if (domainDevices != null && domainDevices.ContainsKey(udid))
            {
                deviceFamilyId = domainDevices[udid];
            }

            return deviceFamilyId;
        }
    }
}
