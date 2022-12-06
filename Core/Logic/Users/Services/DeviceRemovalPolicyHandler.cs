using ApiObjects;
using Phx.Lib.Appconfig;
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
using Phx.Lib.Log;
using System.Reflection;
using ApiObjects.CanaryDeployment;
using ApiObjects.CanaryDeployment.Microservices;
using ApiObjects.DataMigrationEvents;
using CanaryDeploymentManager;
using EventBus.Kafka;

namespace ApiLogic.Users.Services
{
    public class DeviceRemovalPolicyHandler
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());      

        private static readonly Lazy<DeviceRemovalPolicyHandler> lazy = new Lazy<DeviceRemovalPolicyHandler>(() => new DeviceRemovalPolicyHandler());
        public static DeviceRemovalPolicyHandler Instance { get { return lazy.Value; } }

        private readonly CouchbaseManager.CouchbaseManager _cbManager;
        private readonly eCouchbaseBucket _couchbaseBucket;
        private readonly uint _ttl;

        private DeviceRemovalPolicyHandler()
        {
            _ttl = ApplicationConfiguration.Current.UdidUsageConfiguration.TTL.Value;
            bool tryParse = Enum.TryParse(ApplicationConfiguration.Current.UdidUsageConfiguration.BucketName.Value, true, out eCouchbaseBucket _couchbaseBucket);
            if (!tryParse)
                _couchbaseBucket = eCouchbaseBucket.OTT_APPS;

            _cbManager = new CouchbaseManager.CouchbaseManager(_couchbaseBucket);
        }

        public string GetDeviceRemovalCandidate(
            int groupId,
            RollingDevicePolicy rollingDeviceRemovalPolicy,
            int rollingDeviceRemovalFamilyId,
            List<DeviceContainer> deviceFamilies)
        {
            if (rollingDeviceRemovalPolicy == RollingDevicePolicy.NONE)
            {
                return string.Empty;
            }

            // don't use devices from families that are not in the policy
            // map device usage key to uuid
            var deviceUuidUsageKeyToDevice = deviceFamilies
                .Where(x => x.m_deviceFamilyID == rollingDeviceRemovalFamilyId)
                .SelectMany(x => x.DeviceInstances)
                .ToDictionary(key => key.m_deviceUDID, value => value);
            if (!deviceUuidUsageKeyToDevice.Any())
            {
                return string.Empty;
            }
            
            string removalCandidateDeviceUDID = string.Empty;
            switch (rollingDeviceRemovalPolicy)
            {
                case RollingDevicePolicy.LIFO:
                    removalCandidateDeviceUDID = deviceUuidUsageKeyToDevice.OrderBy(x => x.Value.m_activationDate).Last().Key;
                    break;
                case RollingDevicePolicy.FIFO:
                    removalCandidateDeviceUDID = deviceUuidUsageKeyToDevice.OrderBy(x => x.Value.m_activationDate).First().Key;
                    break;
                case RollingDevicePolicy.ACTIVE_DEVICE_ASCENDING:
                    // map device usage to usage date

                    if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.AuthenticationDeviceLoginHistory))
                    {
                        var authClient = AuthenticationClient.GetClientFromTCM();
                        var deviceLoginRecords = authClient.ListDevicesLoginHistory(groupId, deviceUuidUsageKeyToDevice.Keys);
                        if (deviceLoginRecords == null)
                        {
                            _logger.Error($"could not fetch device login records groupId:[{groupId}]]");
                            break;
                        }

                        var deviceUdidsUsageKeysReturned = deviceLoginRecords.Select(d => d.UDID);
                        // first check if there such old devices that do not have a login recotrd at all because of TTL
                        var nonExistingDevices = deviceUuidUsageKeyToDevice.Keys.Except(deviceUdidsUsageKeysReturned).ToList();
                        if (nonExistingDevices.Any())
                        {
                            removalCandidateDeviceUDID = nonExistingDevices.First();
                        }
                        else // if no non existing devices then we can use the min login date..
                        {
                            var minLoginRecord = deviceLoginRecords.MinBy(r => r.LastLoginDate).FirstOrDefault();
                            if (minLoginRecord != null)
                            {
                                removalCandidateDeviceUDID = minLoginRecord.UDID;
                            }
                        }
                    }
                    else
                    { 
                        string removalCandidateDeviceUsageKey = string.Empty;
                        var udids =  deviceUuidUsageKeyToDevice.Keys.ToDictionary(key => GetDomainDeviceUsageDateKey(key), key => key);
                        var loginRecords = _cbManager.GetValues<string>(udids.Keys.ToList(), shouldAllowPartialQuery: true);
                        if (loginRecords?.Any() != true)
                        {
                            _logger.Error($"could not fetch device login records groupId:[{groupId}]");
                            break;
                        }

                        // first check if there such old devices that do not have a login record at all because of TTL
                        var nonExistingDevices = udids.Keys.Except(loginRecords.Keys);
                        if (nonExistingDevices.Any())
                        {
                            removalCandidateDeviceUsageKey = nonExistingDevices.First();
                        }
                        else  // if no non existing devices then we can use the min login date..
                        {
                            removalCandidateDeviceUsageKey = loginRecords.MinBy(r => long.Parse(r.Value)).FirstOrDefault().Key;
                        }

                        removalCandidateDeviceUDID = udids.ContainsKey(removalCandidateDeviceUsageKey)
                            ? udids[removalCandidateDeviceUsageKey]
                            : string.Empty;
                    }

                    break;
            }

            return removalCandidateDeviceUDID;
        }


        public long? GetUdidLastActivity(int groupId, string UDID)
        {
            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.AuthenticationDeviceLoginHistory))
            {
                var authClient = AuthenticationClient.GetClientFromTCM();
                var loginHistoryList = authClient.ListDevicesLoginHistory(groupId, new List<string>() { UDID });

                if (loginHistoryList != null && loginHistoryList.Count() > 0)
                {
                    return loginHistoryList.FirstOrDefault().LastLoginDate;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                var key = GetDomainDeviceUsageDateKey(UDID);
                var deviceLoginTime = UtilsDal.GetObjectFromCB<long?>(_couchbaseBucket, key);
                if (!deviceLoginTime.HasValue)
                {
                    _logger.Debug($"Could not fetch device ([{UDID}]) login record groupId:[{groupId}]]");
                    return null;
                }
                return deviceLoginTime.Value;
            }
        }

        public void SaveDomainDeviceUsageDate(string UDID, int groupId)
        {
            if (string.IsNullOrEmpty(UDID))
            {
                return;
            }

            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.AuthenticationDeviceLoginHistory))
            {
                var authClient = AuthenticationClient.GetClientFromTCM();
                authClient.RecordDeviceSuccessfulLogin(groupId, UDID);
            }
            else
            {
                var now = DateUtils.GetUtcUnixTimestampNow();
                UtilsDal.SaveObjectInCB<long>(_couchbaseBucket, GetDomainDeviceUsageDateKey(UDID), now, true, _ttl);

                if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsEnabledMigrationEvent(groupId, CanaryDeploymentMigrationEvent.DeviceLoginHistory))
                {
                    var migrationEvent = new ApiObjects.DataMigrationEvents.DeviceLoginHistory
                    {
                        Operation = eMigrationOperation.Update,
                        GroupId = groupId,
                        Udid = UDID,
                        LastLoginDate = now,
                    };
                    
                    KafkaPublisher.GetFromTcmConfiguration(migrationEvent).Publish(migrationEvent);

                }
            }
        }

        public void DeleteDomainDeviceUsageDate(string UDID, int groupId)
        {
            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.AuthenticationDeviceLoginHistory))
            {
                AuthenticationClient.GetClientFromTCM().DeleteDomainDeviceUsageDate(groupId, UDID);
            }
            else
            {
                UtilsDal.DeleteObjectFromCB(_couchbaseBucket, GetDomainDeviceUsageDateKey(UDID));
            }
        }

        private static string GetDomainDeviceUsageDateKey(string udid)
        {
            return $"domain_device_usage_date{udid}";
        }
    }
}