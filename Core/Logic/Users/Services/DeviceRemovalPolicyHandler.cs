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

namespace ApiLogic.Users.Services
{
    public class DeviceRemovalPolicyHandler
    {

        public string GetDeviceRemovalCandidate(RollingDevicePolicy rollingDeviceRemovalPolicy,
            List<int> rollingDeviceRemovalFamilyIds,
            List<DeviceContainer> deviceFamilies)
        {
            if (rollingDeviceRemovalPolicy==RollingDevicePolicy.NONE)
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

            

            var removeCandidate = new KeyValuePair<string, long>();
            switch (rollingDeviceRemovalPolicy)
            {
                case RollingDevicePolicy.LIFO:
                    var maximalDate = deviceUuidUsageKeyToDeviceActivationDate.Values.Max();
                    removeCandidate = deviceUuidUsageKeyToDeviceActivationDate.FirstOrDefault(k => k.Value == maximalDate);
                    break;
                case RollingDevicePolicy.FIFO:
                    var minimalDate = deviceUuidUsageKeyToDeviceActivationDate.Values.Min();
                    removeCandidate = deviceUuidUsageKeyToDeviceActivationDate.FirstOrDefault(k => k.Value == minimalDate);
                    break;
                case RollingDevicePolicy.ACTIVE_DEVICE_ASCENDING:
                    // map device usage to usage date
                    var deviceUsageKeysToUsage = new CouchbaseManager.CouchbaseManager(couchbaseBucket)
                        .GetValues<string>(deviceUuidUsageKeyToDevice.Keys.ToList(), true).ToDictionary(x => x.Key, x => long.Parse(x.Value));
                    var asc = deviceUsageKeysToUsage.Values.Min();
                    removeCandidate = deviceUsageKeysToUsage.FirstOrDefault(k => k.Value == asc);
                    break;
                case RollingDevicePolicy.NONE:
                    break;
            }

            return deviceUuidUsageKeyToDevice.ContainsKey(removeCandidate.Key)
                ? deviceUuidUsageKeyToDevice[removeCandidate.Key].m_deviceUDID
                : string.Empty;

        }


        public void SaveDomainDeviceUsageDate(string UDID,int groupId)
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

            UtilsDal.SaveObjectInCB<long>(couchbaseBucket,
                GetDomainDeviceUsageDateKey(UDID),
                DateUtils.GetUtcUnixTimestampNow(),
                true,
                ApplicationConfiguration.Current.UdidUsageConfiguration.TTL.Value);
        }

        public static string GetDomainDeviceUsageDateKey(string udid)
        {
            return $"domain_device_usage_date{udid}";
        }

        
    }
}
