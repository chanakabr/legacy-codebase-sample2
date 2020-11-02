using ApiObjects;
using ConfigurationManager;
using Core.Users;
using CouchbaseManager;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using TVinciShared;
using ApiLogic.Api.Managers;
using AuthenticationGrpcClientWrapper;
using MoreLinq;
using Grpc;
using KLogMonitor;
using System.Reflection;

namespace ApiLogic.Users.Services
{
    public class DeviceRemovalPolicyHandler
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string GetDeviceRemovalCandidate(int groupId, RollingDevicePolicy rollingDeviceRemovalPolicy,
            List<int> rollingDeviceRemovalFamilyIds,
            List<DeviceContainer> deviceFamilies)
        {
            if (rollingDeviceRemovalPolicy == RollingDevicePolicy.NONE)
                return string.Empty;


            var tryParse = Enum.TryParse(ApplicationConfiguration.Current.UdidUsageConfiguration.BucketName.Value,
                true,
                out eCouchbaseBucket couchbaseBucket);

            if (!tryParse)
                couchbaseBucket = eCouchbaseBucket.OTT_APPS;

            // map device usage key to uuid 
            //don't map family ids that are not in the policy
            var deviceUuidUsageKeyToDevice = deviceFamilies.SelectMany(x => x.DeviceInstances)
                .Where(x => rollingDeviceRemovalFamilyIds.Contains(x.m_deviceFamilyID))
                .ToDictionary(key => GetDomainDeviceUsageDateKey(key.m_deviceUDID),
                    value => value);

            // map device usage key to activation date
            //don't map family ids that are not in the policy
            var deviceUuidUsageKeyToDeviceActivationDate = deviceFamilies.SelectMany(x => x.DeviceInstances)
                .Where(x => rollingDeviceRemovalFamilyIds.Contains(x.m_deviceFamilyID))
                .ToDictionary(key => GetDomainDeviceUsageDateKey(key.m_deviceUDID),
                    value => value.m_activationDate.ToUtcUnixTimestampSeconds());


            var removalCandidateDeviceUsageKey = "";
            switch (rollingDeviceRemovalPolicy)
            {
                case RollingDevicePolicy.LIFO:
                    var maximalDate = deviceUuidUsageKeyToDeviceActivationDate.Values.Max();
                    removalCandidateDeviceUsageKey = deviceUuidUsageKeyToDeviceActivationDate.FirstOrDefault(k => k.Value == maximalDate).Key;
                    break;
                case RollingDevicePolicy.FIFO:
                    var minimalDate = deviceUuidUsageKeyToDeviceActivationDate.Values.Min();
                    removalCandidateDeviceUsageKey = deviceUuidUsageKeyToDeviceActivationDate.FirstOrDefault(k => k.Value == minimalDate).Key;
                    break;
                case RollingDevicePolicy.ACTIVE_DEVICE_ASCENDING:
                    // map device usage to usage date

                    if (ApplicationConfiguration.Current.MicroservicesClientConfiguration.Authentication.DataOwnershipConfiguration.DeviceLoginHistory.Value)
                    {
                        var authClient = AuthenticationClient.GetClientFromTCM();
                        var deviceLoginRecords = authClient.ListDevicesLoginHistory(groupId, deviceUuidUsageKeyToDevice.Keys);
                        if (deviceLoginRecords == null)
                        {
                            _logger.Error($"could not fetch device login records groupId:[{groupId}]]");
                            break;
                        }

                        var deviceUdidsReturned = deviceLoginRecords.Select(d => d.UDID);
                        // first check if there such old devices that do not have a login recotrd at all because of TTL
                        var nonExistingDevices = deviceUuidUsageKeyToDevice.Keys.Except(deviceUdidsReturned).ToList();
                        if (nonExistingDevices.Any())
                        {
                            removalCandidateDeviceUsageKey = GetDomainDeviceUsageDateKey(nonExistingDevices.First());
                        }
                        else // if no non existing devices then we can use the min login date..
                        {
                            var minLoginRecord = deviceLoginRecords.MinBy(r => r.LastLoginDate).FirstOrDefault();
                            if (minLoginRecord != null)
                            {
                                removalCandidateDeviceUsageKey = GetDomainDeviceUsageDateKey(minLoginRecord.UDID);
                            }
                        }
                    }
                    else
                    {
                        var cbMgr = new CouchbaseManager.CouchbaseManager(couchbaseBucket);
                        var loginRecords = cbMgr.GetValues<string>(deviceUuidUsageKeyToDevice.Keys.ToList(), shouldAllowPartialQuery: true);
                        if (loginRecords?.Any() != true)
                        {
                            _logger.Error($"could not fetch device login records groupId:[{groupId}]");
                            break;
                        }

                        // first check if there such old devices that do not have a login recotrd at all because of TTL
                        var nonExistingDevices = deviceUuidUsageKeyToDevice.Keys.Except(loginRecords.Keys);
                        if (nonExistingDevices.Any())
                        {
                            removalCandidateDeviceUsageKey = GetDomainDeviceUsageDateKey(nonExistingDevices.First());
                        }
                        else  // if no non existing devices then we can use the min login date..
                        {
                            removalCandidateDeviceUsageKey = loginRecords.MinBy(r => long.Parse(r.Value)).FirstOrDefault().Key;
                        }
                    }

                    break;
                case RollingDevicePolicy.NONE:
                    break;
            }

            return deviceUuidUsageKeyToDevice.ContainsKey(removalCandidateDeviceUsageKey)
                ? deviceUuidUsageKeyToDevice[removalCandidateDeviceUsageKey].m_deviceUDID
                : string.Empty;
        }


        public void SaveDomainDeviceUsageDate(string UDID, int groupId)
        {
            if (!PartnerConfigurationManager.GetGeneralPartnerConfiguration(groupId).HasObjects())
                return;

            var generalPartnerConfig = PartnerConfigurationManager.GetGeneralPartnerConfiguration(groupId).Objects.FirstOrDefault();

            if (generalPartnerConfig?.RollingDeviceRemovalData.RollingDeviceRemovalPolicy == null ||
                generalPartnerConfig.RollingDeviceRemovalData.RollingDeviceRemovalFamilyIds.Count <= 0)
                return;

            if (generalPartnerConfig?.RollingDeviceRemovalData.RollingDeviceRemovalPolicy != RollingDevicePolicy.ACTIVE_DEVICE_ASCENDING)
                return;


            var tryParse = Enum.TryParse(ApplicationConfiguration.Current.UdidUsageConfiguration.BucketName.Value,
                true,
                out eCouchbaseBucket couchbaseBucket);

            if (!tryParse)
            {
                couchbaseBucket = eCouchbaseBucket.OTT_APPS;
            }

            if (ApplicationConfiguration.Current.MicroservicesClientConfiguration.Authentication.DataOwnershipConfiguration.DeviceLoginHistory.Value)
            {
                var authClient = AuthenticationClient.GetClientFromTCM();
                authClient.RecordDeviceSuccessfulLogin(groupId, UDID);
            }
            else
            {
                UtilsDal.SaveObjectInCB<long>(couchbaseBucket,
                    GetDomainDeviceUsageDateKey(UDID),
                    DateUtils.GetUtcUnixTimestampNow(),
                    true,
                    ApplicationConfiguration.Current.UdidUsageConfiguration.TTL.Value);
            }
        }

        private static string GetDomainDeviceUsageDateKey(string udid)
        {
            return $"domain_device_usage_date{udid}";
        }
    }
}