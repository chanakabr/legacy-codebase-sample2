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

        public static DomainResponseStatus Validate(DevicePlayData devicePlayData, Domain domain, int groupId)
        {
            DomainResponseStatus status = DomainResponseStatus.UnKnown;

            if (devicePlayData.MediaConcurrencyRuleIds != null && devicePlayData.MediaConcurrencyRuleIds.Count > 0)
            {
                status = ValidateMediaConcurrency(devicePlayData, groupId);
            }

            if (devicePlayData.AssetMediaConcurrencyRuleIds != null && devicePlayData.AssetMediaConcurrencyRuleIds.Count > 0 &&
                (status == DomainResponseStatus.OK || status == DomainResponseStatus.UnKnown))
            {
                status = ValidateAssetRulesConcurrency(devicePlayData, groupId, eAssetTypes.MEDIA);
            }

            if (devicePlayData.AssetEpgConcurrencyRuleIds != null && devicePlayData.AssetEpgConcurrencyRuleIds.Count > 0 &&
                (status == DomainResponseStatus.OK || status == DomainResponseStatus.UnKnown))
            {
                status = ValidateAssetRulesConcurrency(devicePlayData, groupId, eAssetTypes.EPG);
            }

            // if it's MediaConcurrencyLimitation no need to check this one 
            if (status == DomainResponseStatus.OK || status == DomainResponseStatus.UnKnown)
            {
                status = ValidateDeviceFamilyConcurrency(devicePlayData, groupId, domain);
            }

            if (status == DomainResponseStatus.ConcurrencyLimitation || status == DomainResponseStatus.MediaConcurrencyLimitation)
            {
                CatalogDAL.DeleteDevicePlayData(devicePlayData.UDID);
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
        private static DomainResponseStatus ValidateMediaConcurrency(DevicePlayData devicePlayData, int groupId)
        {
            if (devicePlayData.MediaConcurrencyRuleIds == null || devicePlayData.MediaConcurrencyRuleIds.Count == 0)
            {
                return DomainResponseStatus.OK;
            }

            log.DebugFormat("ValidateAssetConcurrency, domainId:{0}, mediaId:{1}, ruleIds:{2}",
                            devicePlayData.DomainId, devicePlayData.AssetId, string.Join(",", devicePlayData.MediaConcurrencyRuleIds));

            try
            {
                // Get all domain media marks
                List<DevicePlayData> devicePlayDataList = CatalogDAL.GetDevicePlayDataList(GetDomainDevices(devicePlayData.DomainId, groupId),
                                                                                       new List<ePlayType>() { ePlayType.NPVR, ePlayType.MEDIA },
                                                                                       Utils.CONCURRENCY_MILLISEC_THRESHOLD, devicePlayData.UDID);

                if (devicePlayDataList == null || devicePlayDataList.Count == 0)
                {
                    return DomainResponseStatus.OK;
                }

                // Get all group's media concurrency rules
                var groupConcurrencyRules = Api.api.GetGroupMediaConcurrencyRules(groupId);

                if (groupConcurrencyRules == null || groupConcurrencyRules.Count == 0)
                {
                    return DomainResponseStatus.OK;
                }

                foreach (int ruleId in devicePlayData.MediaConcurrencyRuleIds)
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

                    List<DevicePlayData> otherDevicePlayData = null;

                    switch (policy)
                    {
                        case ConcurrencyRestrictionPolicy.Single:
                            {
                                otherDevicePlayData = devicePlayDataList.FindAll(x => x.AssetId == devicePlayData.AssetId);
                                break;
                            }
                        case ConcurrencyRestrictionPolicy.Group:
                            {
                                otherDevicePlayData = devicePlayDataList.FindAll(x => x.MediaConcurrencyRuleIds != null && x.MediaConcurrencyRuleIds.Contains(ruleId));
                                break;
                            }
                        default:
                            break;
                    }

                    if (otherDevicePlayData != null && otherDevicePlayData.Count >= mediaConcurrencyLimit)
                    {
                        log.DebugFormat("MediaConcurrencyLimitation, domainId:{0}, mediaId:{1}, ruleId:{2}, limit:{3}, count:{4}",
                           devicePlayData.DomainId, devicePlayData.AssetId, ruleId, mediaConcurrencyLimit, otherDevicePlayData.Count);

                        if (CheckDeviceConcurrencyPrioritization(groupId, devicePlayData, otherDevicePlayData) == DomainResponseStatus.ConcurrencyLimitation)
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

        private static DomainResponseStatus ValidateAssetRulesConcurrency(DevicePlayData devicePlayData, int groupId, eAssetTypes assetType)
        {
            List<long> assetRuleIds;
            long assetId;

            if (assetType == eAssetTypes.MEDIA)
            {
                assetRuleIds = devicePlayData.AssetMediaConcurrencyRuleIds;
                assetId = devicePlayData.AssetId;
            }
            else
            {
                assetRuleIds = devicePlayData.AssetEpgConcurrencyRuleIds;
                assetId = devicePlayData.ProgramId;
            }

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

                List<DevicePlayData> devicePlayDataList =
                    CatalogDAL.GetDevicePlayDataList(GetDomainDevices(devicePlayData.DomainId, groupId), playTypes, Utils.CONCURRENCY_MILLISEC_THRESHOLD, devicePlayData.UDID);

                if (devicePlayDataList == null || devicePlayDataList.Count == 0)
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
                        assetDevicePlayData = GetAssetMediaDevicePlayDataByRestrictionPolicy(concurrencyCondition.RestrictionPolicy, devicePlayDataList, assetId, currAssetRuleId);
                    }
                    else if (assetType == eAssetTypes.EPG)
                    {
                        assetDevicePlayData = GetAssetEpgDevicePlayDataByRestrictionPolicy(concurrencyCondition.RestrictionPolicy, devicePlayDataList, assetId, currAssetRuleId);
                    }


                    if (assetDevicePlayData != null && assetDevicePlayData.Count >= concurrencyCondition.Limit)
                    {
                        log.DebugFormat("ValidateAssetRulesConcurrency, domainId:{0}, mediaId:{1}, assetRuleId:{2}, limit:{3}, count:{4}",
                                        devicePlayData.DomainId, assetId, currAssetRuleId, concurrencyCondition.Limit, assetDevicePlayData.Count);

                        if (CheckDeviceConcurrencyPrioritization(groupId, devicePlayData, assetDevicePlayData) == DomainResponseStatus.ConcurrencyLimitation)
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

        private static List<DevicePlayData> GetAssetMediaDevicePlayDataByRestrictionPolicy(ConcurrencyRestrictionPolicy restrictionPolicy, List<DevicePlayData> devicePlayData,
                                                                                           long mediaId, int assetRuleId)
        {
            List<DevicePlayData> assetDevicePlayData = null;

            switch (restrictionPolicy)
            {
                case ConcurrencyRestrictionPolicy.Single:
                    {
                        assetDevicePlayData = devicePlayData.FindAll(x => x.AssetId == mediaId);
                        break;
                    }
                case ConcurrencyRestrictionPolicy.Group:
                    {
                        assetDevicePlayData = devicePlayData.FindAll(x => x.AssetMediaConcurrencyRuleIds != null && x.AssetMediaConcurrencyRuleIds.Contains(assetRuleId));
                        break;
                    }
                default:
                    break;
            }

            return assetDevicePlayData;
        }

        private static List<DevicePlayData> GetAssetEpgDevicePlayDataByRestrictionPolicy(ConcurrencyRestrictionPolicy restrictionPolicy, List<DevicePlayData> devicePlayData,
                                                                                         long programId, int assetRuleId)
        {
            List<DevicePlayData> assetDevicePlayData = null;

            switch (restrictionPolicy)
            {
                case ConcurrencyRestrictionPolicy.Single:
                    {
                        assetDevicePlayData = devicePlayData.FindAll(x => x.ProgramId == programId);
                        break;
                    }
                case ConcurrencyRestrictionPolicy.Group:
                    {
                        assetDevicePlayData = devicePlayData.FindAll(x => x.AssetEpgConcurrencyRuleIds != null && x.AssetEpgConcurrencyRuleIds.Contains(assetRuleId));
                        break;
                    }
                default:
                    break;
            }

            return assetDevicePlayData;
        }

        private static DomainResponseStatus ValidateDeviceFamilyConcurrency(DevicePlayData devicePlayData, int groupId, Domain domain)
        {
            if (string.IsNullOrEmpty(devicePlayData.UDID))
            {
                return DomainResponseStatus.OK;
            }

            if (devicePlayData.DeviceFamilyId == 0)
            {
                return DomainResponseStatus.DeviceTypeNotAllowed;
            }

            if (domain.m_oLimitationsManager.Concurrency <= 0 || domain.IsAgnosticToDeviceLimitation(ValidationType.Concurrency, devicePlayData.DeviceFamilyId))
            {
                // there are no concurrency limitations at all.
                return DomainResponseStatus.OK;
            }

            int concurrentDeviceFamilyIdCount = 0;
            List<DevicePlayData> devicePlayDataStreams = domain.GetConcurrentCount(devicePlayData.UDID, ref concurrentDeviceFamilyIdCount, devicePlayData.DeviceFamilyId);
            if (devicePlayDataStreams == null)
            {
                // no active streams at all
                return DomainResponseStatus.OK;
            }

            if (devicePlayDataStreams.Count >= domain.m_oLimitationsManager.Concurrency)
            {
                return CheckDeviceConcurrencyPrioritization(groupId, devicePlayData, devicePlayDataStreams);
            }

            if (concurrentDeviceFamilyIdCount == 0)
            {
                // no active streams at the device's family.
                return DomainResponseStatus.OK;
            }

            DeviceContainer deviceContainer = domain.GetDeviceContainerByUdid(devicePlayData.UDID);
            if (deviceContainer != null &&
                concurrentDeviceFamilyIdCount >= deviceContainer.m_oLimitationsManager.Concurrency)
            {
                // device family reached its max limit. Cannot allow a new stream
                return DomainResponseStatus.ConcurrencyLimitation;
            }

            // User is able to watch through this device. Hasn't reach the device family max limitation
            return DomainResponseStatus.OK;
        }

        private static DomainResponseStatus CheckDeviceConcurrencyPrioritization(int groupId, DevicePlayData currDevicePlayData, List<DevicePlayData> otherDeviceFamilyIds)
        {
            DeviceConcurrencyPriority deviceConcurrencyPriority = Api.api.GetDeviceConcurrencyPriority(groupId);

            if (deviceConcurrencyPriority != null)
            {
                int currDevicePriorityIndex = deviceConcurrencyPriority.DeviceFamilyIds.IndexOf(currDevicePlayData.DeviceFamilyId);

                if (currDevicePriorityIndex != -1)
                {
                    List<DevicePlayData> devicesWithSameFamilyId = new List<DevicePlayData>();

                    foreach (var otherDeviceFamilyId in otherDeviceFamilyIds.OrderBy(x => x.CreatedAt))
                    {
                        if (currDevicePlayData.DeviceFamilyId == otherDeviceFamilyId.DeviceFamilyId)
                        {
                            devicesWithSameFamilyId.Add(otherDeviceFamilyId);
                        }

                        int otherDevicePriorityIndex = deviceConcurrencyPriority.DeviceFamilyIds.IndexOf(otherDeviceFamilyId.DeviceFamilyId);
                        if (otherDevicePriorityIndex == -1 || currDevicePriorityIndex < otherDevicePriorityIndex)
                        {
                            return DomainResponseStatus.OK;
                        }
                    }

                    if (devicesWithSameFamilyId.Count > 0)
                    {
                        if (deviceConcurrencyPriority.PriorityOrder == DowngradePolicy.FIFO &&
                            (devicesWithSameFamilyId[0].CreatedAt < currDevicePlayData.CreatedAt))
                        {
                            return DomainResponseStatus.OK;
                        }

                        if (deviceConcurrencyPriority.PriorityOrder == DowngradePolicy.LIFO &&
                           devicesWithSameFamilyId[devicesWithSameFamilyId.Count - 1].CreatedAt > currDevicePlayData.CreatedAt &&
                           currDevicePlayData.CreatedAt != 0)
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