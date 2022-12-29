using Core.Users;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Domains;
using AutoMapper.Configuration;
using TVinciShared;
using WebAPI.Models.Pricing;
using ApiObjects.Pricing;
using ApiObjects;
using WebAPI.Models.API;
using System.Linq;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.ModelsValidators;

namespace WebAPI.Mapping.ObjectsConvertor
{
    public class DomainsMappings
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            //Device
            cfg.CreateMap<Device, KalturaHouseholdDevice>()
                .ForMember(dest => dest.Udid, opt => opt.MapFrom(src => src.m_deviceUDID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_deviceName))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.m_deviceBrand))
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.m_deviceBrandID))
                .ForMember(dest => dest.ActivatedOn, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_activationDate)))
                .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => ConvertDeviceStatus(src.m_state)))
                .ForMember(dest => dest.State, opt => opt.ResolveUsing(src => ConvertDeviceState(src.m_state)))
                .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.m_domainID))
                .ForMember(dest => dest.Drm, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.LicenseData) ?
                    new KalturaCustomDrmPlaybackPluginData(null, false) { Data = src.LicenseData, Scheme = KalturaDrmSchemeName.CUSTOM_DRM } : null))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId))
                .ForMember(dest => dest.MacAddress, opt => opt.MapFrom(src => src.MacAddress))
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model))
                .ForMember(dest => dest.Manufacturer, opt => opt.MapFrom(src => src.Manufacturer))
                .ForMember(dest => dest.ManufacturerId, opt => opt.MapFrom(src => src.ManufacturerId))
                .ForMember(dest => dest.DeviceFamilyId, opt => opt.MapFrom(src => src.m_deviceFamilyID))
                .ForMember(dest => dest.LastActivityTime, opt => opt.MapFrom(src => src.LastActivityTime))
                .ForMember(dest => dest.DynamicData, opt => opt.ResolveUsing(src => Utils.Utils.ConvertToSerializableDictionary(src.DynamicData)))
                .AfterMap((src, dest) => dest.DynamicData = src.DynamicData != null ? dest.DynamicData : null)
                ;

            cfg.CreateMap<Device, KalturaDevice>()
                .ForMember(dest => dest.Udid, opt => opt.MapFrom(src => src.m_deviceUDID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_deviceName))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.m_deviceBrand))
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.m_deviceBrandID))
                .ForMember(dest => dest.ActivatedOn, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_activationDate)))
                .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => ConvertDeviceStatus(src.m_state)))
                .ForMember(dest => dest.State, opt => opt.ResolveUsing(src => ConvertDeviceState(src.m_state)))
                .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.m_domainID))
                .AfterMap((src, dest) => dest.DynamicData = src.DynamicData != null ? dest.DynamicData : null)
                ;

            cfg.CreateMap<KeyValuePair, KalturaDynamicData>()
                .ConvertUsing(src => new KalturaDynamicData(src.key, new KalturaStringValue { value = src.value }));

            //HomeNetwork
            cfg.CreateMap<HomeNetwork, KalturaHomeNetwork>()
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.UID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            //DeviceContainer to KalturaHouseholdDeviceFamily
            cfg.CreateMap<DeviceContainer, KalturaDeviceFamily>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_deviceFamilyID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_deviceFamilyName))
                .ForMember(dest => dest.DeviceLimit, opt => opt.MapFrom(src => src.m_deviceLimit))
                .ForMember(dest => dest.ConcurrentLimit, opt => opt.MapFrom(src => src.m_deviceConcurrentLimit))
                .ForMember(dest => dest.Devices, opt => opt.MapFrom(src => src.DeviceInstances))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_deviceFamilyID));

            //DeviceFamilyLimitations to KalturaDeviceFamily
            cfg.CreateMap<DeviceFamilyLimitations, KalturaHouseholdDeviceFamilyLimitations>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.deviceFamily))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.deviceFamilyName))
                .ForMember(dest => dest.DeviceLimit, opt => opt.MapFrom(src => src.quantity))
                .ForMember(dest => dest.ConcurrentLimit, opt => opt.MapFrom(src => src.concurrency))
                .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.Frequency))
                .ForMember(dest => dest.IsDefaultConcurrentLimit, opt => opt.MapFrom(src => src.isDefaultConcurrency))
                .ForMember(dest => dest.IsDefaultDeviceLimit, opt => opt.MapFrom(src => src.isDefaultQuantity))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.deviceFamily));

            //KalturaHouseholdDeviceFamilyLimitations DeviceFamilyLimitations>
            cfg.CreateMap<KalturaHouseholdDeviceFamilyLimitations, DeviceFamilyLimitations>()
                .ForMember(dest => dest.deviceFamily, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.concurrency, opt => opt.MapFrom(src => src.ConcurrentLimit ?? -1))
                .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.Frequency ?? -1))
                .ForMember(dest => dest.quantity, opt => opt.MapFrom(src => src.DeviceLimit ?? -1));

            //Domain
            cfg.CreateMap<Domain, KalturaHousehold>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nDomainID))
                .ForMember(dest => dest.DefaultUsers, opt => opt.MapFrom(src => src.m_DefaultUsersIDs))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_sDescription))
                .ForMember(dest => dest.ConcurrentLimit, opt => opt.MapFrom(src => src.m_nConcurrentLimit))
                .ForMember(dest => dest.DevicesLimit, opt => opt.MapFrom(src => src.m_nDeviceLimit))
                .ForMember(dest => dest.HouseholdLimitationsId, opt => opt.MapFrom(src => src.m_nLimit))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.m_sCoGuid))
                .ForMember(dest => dest.FrequencyNextDeviceAction, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_NextActionFreq)))
                .ForMember(dest => dest.FrequencyNextUserAction, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_NextUserActionFreq)))
                .ForMember(dest => dest.IsFrequencyEnabled, opt => opt.MapFrom(src => src.m_frequencyFlag))
                .ForMember(dest => dest.MasterUsers, opt => opt.MapFrom(src => src.m_masterGUIDs))
                .ForMember(dest => dest.PendingUsers, opt => opt.MapFrom(src => src.m_PendingUsersIDs))
                .ForMember(dest => dest.RegionId, opt => opt.MapFrom(src => src.m_nRegion))
                .ForMember(dest => dest.Restriction, opt => opt.ResolveUsing(src => ConvertDomainRestriction(src.m_DomainRestriction)))
                .ForMember(dest => dest.State, opt => opt.ResolveUsing(src => ConvertDomainStatus(src.m_DomainStatus)))
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.m_UsersIDs))
                .ForMember(dest => dest.UsersLimit, opt => opt.MapFrom(src => src.m_nUserLimit))
                .ForMember(dest => dest.DeviceFamilies, opt => opt.MapFrom(src => src.m_deviceFamilies))
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.roleId))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.CreateDate)))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.UpdateDate)));

            //string (pin) to KalturaDevicePin
            cfg.CreateMap<string, KalturaDevicePin>()
                .ForMember(dest => dest.Pin, opt => opt.MapFrom(src => src));

            //DLM to KalturaHouseholdLimitationModule
            cfg.CreateMap<LimitationsManager, KalturaHouseholdLimitations>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.domianLimitID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.DomainLimitName))
                .ForMember(dest => dest.ConcurrentLimit, opt => opt.MapFrom(src => src.Concurrency))
                .ForMember(dest => dest.DeviceFrequency, opt => opt.MapFrom(src => src.Frequency))
                .ForMember(dest => dest.DeviceFrequencyDescription, opt => opt.MapFrom(src => src.FrequencyDescription))
                .ForMember(dest => dest.DeviceLimit, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.NpvrQuotaInSeconds, opt => opt.MapFrom(src => src.npvrQuotaInSecs))
                .ForMember(dest => dest.UsersLimit, opt => opt.MapFrom(src => src.nUserLimit))
                .ForMember(dest => dest.UserFrequency, opt => opt.MapFrom(src => src.UserFrequency))
                .ForMember(dest => dest.UserFrequencyDescription, opt => opt.MapFrom(src => src.UserFrequencyDescrition))
                .ForMember(dest => dest.AssociatedDeviceFamiliesIdsIn, opt => opt.MapFrom(src => src.CreateAssociatedDeviceFamiliesFromLimitation()))
                .ForMember(dest => dest.DeviceFamiliesLimitations, opt => opt.MapFrom(src => src.lDeviceFamilyLimitations))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            
            //KalturaHouseholdLimitationModule to DLM
            cfg.CreateMap<KalturaHouseholdLimitations, LimitationsManager>()
                .ForMember(dest => dest.domianLimitID, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Concurrency, opt => opt.MapFrom(src => src.ConcurrentLimit))
                .ForMember(dest => dest.lDeviceFamilyLimitations, opt => opt.MapFrom(src => src.DeviceFamiliesLimitations.Concat(src.AssociatedDeviceFamiliesToLimitations())))
                .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.DeviceFrequency))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.DeviceLimit))
                .ForMember(dest => dest.DomainLimitName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.npvrQuotaInSecs, opt => opt.MapFrom(src => src.NpvrQuotaInSeconds))
                .ForMember(dest => dest.UserFrequency, opt => opt.MapFrom(src => src.UserFrequency))
                .ForMember(dest => dest.nUserLimit, opt => opt.MapFrom(src => src.UsersLimit))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            //DomainDevice
            cfg.CreateMap<DomainDevice, KalturaHouseholdDevice>()
                .ForMember(dest => dest.Udid, opt => opt.MapFrom(src => src.Udid))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => ConvertDeviceStatus(src.ActivataionStatus)))
                .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.DomainId))
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.DeviceBrandId))
                .ForMember(dest => dest.ActivatedOn, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.ActivatedOn)))
                .ForMember(dest => dest.DeviceFamilyId, opt => opt.MapFrom(src => src.DeviceFamilyId))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId))
                .ForMember(dest => dest.MacAddress, opt => opt.MapFrom(src => src.MacAddress))
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model))
                .ForMember(dest => dest.Manufacturer, opt => opt.MapFrom(src => src.Manufacturer))
                .ForMember(dest => dest.ManufacturerId, opt => opt.MapFrom(src => src.ManufacturerId))
                .ForMember(dest => dest.LastActivityTime, opt => opt.MapFrom(src => src.LastActivityTime))
                .ForMember(dest => dest.DynamicData, opt => opt.ResolveUsing(src => Utils.Utils.ConvertToSerializableDictionary(src.DynamicData)))
                .AfterMap((src, dest) => dest.DynamicData = src.DynamicData != null ? dest.DynamicData : null)
            ;

            cfg.CreateMap<KalturaHouseholdDevice, DomainDevice>()
                .ForMember(dest => dest.Udid, opt => opt.MapFrom(src => src.Udid))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DomainId, opt => opt.MapFrom(src => src.HouseholdId))
                .ForMember(dest => dest.DeviceBrandId, opt => opt.ResolveUsing(src => src.BrandId ?? 0))
                .ForMember(dest => dest.DeviceFamilyId, opt => opt.MapFrom(src => src.DeviceFamilyId))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId))
                .ForMember(dest => dest.MacAddress, opt => opt.MapFrom(src => src.MacAddress))
                .ForMember(dest => dest.LastActivityTime, opt => opt.MapFrom(src => src.LastActivityTime))
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model))
                .ForMember(dest => dest.Manufacturer, opt => opt.MapFrom(src => src.Manufacturer))
                .ForMember(dest => dest.ManufacturerId, opt => opt.MapFrom(src => src.ManufacturerId))
                .ForMember(dest => dest.DynamicData, opt => opt.ResolveUsing(src => src.DynamicData?.ToDictionary(x => x.Key, x => x.Value?.value)))
                .AfterMap((src, dest) => dest.DynamicData = src.DynamicData != null ? dest.DynamicData : null)
                ;

            //CouponWallet, KalturaHouseholdCoupon
            cfg.CreateMap<ApiObjects.Pricing.CouponWallet, KalturaHouseholdCoupon>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.CouponCode))
                .ForMember(dest => dest.LastUsageDate, opt => opt.MapFrom(src => src.LastUsageDate))
                ;

            //KalturaHouseholdCoupon, CouponWallet
            cfg.CreateMap<KalturaHouseholdCoupon, ApiObjects.Pricing.CouponWallet>()
                .ForMember(dest => dest.CouponCode, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.LastUsageDate, opt => opt.MapFrom(src => src.LastUsageDate))
                ;

            //KalturaHouseholdCoupon, CouponWallet
            cfg.CreateMap<KalturaHouseholdCouponFilter, ApiObjects.Pricing.CouponWalletFilter>()
                .ForMember(dest => dest.BusinessModuleId, opt => opt.MapFrom(src => src.BusinessModuleIdEqual))
                .ForMember(dest => dest.BusinessModuleType, opt => opt.MapFrom(src => src.BusinessModuleTypeEqual))
                .ForMember(dest => dest.CouponCode, opt => opt.MapFrom(src => src.CouponCode))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                ;

            cfg.CreateMap<KalturaCouponStatus, CouponsStatus>()
                .ConvertUsing(couponStatus =>
                {
                    //KalturaCouponStatus => CouponsStatus
                    switch (couponStatus)
                    {
                        case KalturaCouponStatus.VALID:
                            return CouponsStatus.Valid;
                        case KalturaCouponStatus.NOT_EXISTS:
                            return CouponsStatus.NotExists;
                        case KalturaCouponStatus.ALREADY_USED:
                            return CouponsStatus.AllreadyUsed;
                        case KalturaCouponStatus.EXPIRED:
                            return CouponsStatus.Expired;
                        case KalturaCouponStatus.INACTIVE:
                            return CouponsStatus.NotActive;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown CouponsStatus value : {0}", couponStatus.ToString()));
                    }
                });

            //Iot
            cfg.CreateMap<Iot, KalturaIot>()
                .ForMember(dest => dest.Udid, opt => opt.MapFrom(src => src.Udid))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.UserPassword, opt => opt.MapFrom(src => src.UserPassword))
                .ForMember(dest => dest.ThingArn, opt => opt.MapFrom(src => src.ThingArn))
                .ForMember(dest => dest.ThingId, opt => opt.MapFrom(src => src.ThingId))
                .ForMember(dest => dest.AccessKey, opt => opt.MapFrom(src => src.AccessKey))
                .ForMember(dest => dest.AccessSecretKey, opt => opt.MapFrom(src => src.AccessSecretKey))
                .ForMember(dest => dest.EndPoint, opt => opt.MapFrom(src => src.EndPoint))
                .ForMember(dest => dest.ExtendedEndPoint, opt => opt.MapFrom(src => src.ExtendedEndPoint))
                .ForMember(dest => dest.IdentityId, opt => opt.MapFrom(src => src.IdentityId))
                .ForMember(dest => dest.IdentityPoolId, opt => opt.MapFrom(src => src.IdentityPoolId))
                .ForMember(dest => dest.Principal, opt => opt.MapFrom(src => src.Principal))
                ;

            cfg.CreateMap<KalturaIot, Iot>()
                .ForMember(dest => dest.Udid, opt => opt.MapFrom(src => src.Udid))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.UserPassword, opt => opt.MapFrom(src => src.UserPassword))
                .ForMember(dest => dest.ThingArn, opt => opt.MapFrom(src => src.ThingArn))
                .ForMember(dest => dest.ThingId, opt => opt.MapFrom(src => src.ThingId))
                .ForMember(dest => dest.AccessKey, opt => opt.MapFrom(src => src.AccessKey))
                .ForMember(dest => dest.AccessSecretKey, opt => opt.MapFrom(src => src.AccessSecretKey))
                .ForMember(dest => dest.EndPoint, opt => opt.MapFrom(src => src.EndPoint))
                .ForMember(dest => dest.ExtendedEndPoint, opt => opt.MapFrom(src => src.ExtendedEndPoint))
                .ForMember(dest => dest.IdentityId, opt => opt.MapFrom(src => src.IdentityId))
                .ForMember(dest => dest.IdentityPoolId, opt => opt.MapFrom(src => src.IdentityPoolId))
                .ForMember(dest => dest.Principal, opt => opt.MapFrom(src => src.Principal))
                ;
            
            cfg.CreateMap<CognitoUserPool, KalturaCognitoUserPool>()
                .ForMember(dest => dest.IotDefault, opt => opt.MapFrom(src => src.IotDefault))
                ;

            cfg.CreateMap<KalturaCognitoUserPool, CognitoUserPool>()
                .ForMember(dest => dest.IotDefault, opt => opt.MapFrom(src => src.IotDefault))
                ;

            cfg.CreateMap<CredentialsProvider, KalturaCredentialsProvider>()
               .ForMember(dest => dest.CognitoIdentity, opt => opt.MapFrom(src => src.CognitoIdentity))
               ;

            cfg.CreateMap<KalturaCredentialsProvider, CredentialsProvider>()
                .ForMember(dest => dest.CognitoIdentity, opt => opt.MapFrom(src => src.CognitoIdentity))
                ;

            cfg.CreateMap<CognitoIdentity, KalturaCognitoIdentity>()
              .ForMember(dest => dest.IotDefault, opt => opt.MapFrom(src => src.IotDefault))
              ;

            cfg.CreateMap<KalturaCognitoIdentity, CognitoIdentity>()
                .ForMember(dest => dest.IotDefault, opt => opt.MapFrom(src => src.IotDefault))
                ;

            cfg.CreateMap<IotDefault, KalturaIotDefault>()
               .ForMember(dest => dest.PoolId, opt => opt.MapFrom(src => src.PoolId))
               .ForMember(dest => dest.AppClientId, opt => opt.MapFrom(src => src.AppClientId))
               .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.Region))
                ;

            cfg.CreateMap<KalturaIotDefault, IotDefault>()
               .ForMember(dest => dest.PoolId, opt => opt.MapFrom(src => src.PoolId))
               .ForMember(dest => dest.AppClientId, opt => opt.MapFrom(src => src.AppClientId))
               .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.Region))
                ;


            cfg.CreateMap<KalturaIotProfileAws, IotProfileAws>()
                 .ForMember(dest => dest.AccessKeyId, opt => opt.MapFrom(src => src.AccessKeyId))
                 .ForMember(dest => dest.SecretAccessKey, opt => opt.MapFrom(src => src.SecretAccessKey))
                 .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.ClientId))
                 .ForMember(dest => dest.IdentityPoolId, opt => opt.MapFrom(src => src.IdentityPoolId))
                 .ForMember(dest => dest.IotEndPoint, opt => opt.MapFrom(src => src.IotEndPoint))
                 .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.Region))
                 .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
                 ;
            
            cfg.CreateMap<IotProfileAws, KalturaIotProfileAws>()
                 .ForMember(dest => dest.AccessKeyId, opt => opt.MapFrom(src => src.AccessKeyId))
                 .ForMember(dest => dest.SecretAccessKey, opt => opt.MapFrom(src => src.SecretAccessKey))
                 .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.ClientId))
                 .ForMember(dest => dest.IdentityPoolId, opt => opt.MapFrom(src => src.IdentityPoolId))
                 .ForMember(dest => dest.IotEndPoint, opt => opt.MapFrom(src => src.IotEndPoint))
                 .ForMember(dest => dest.Region, opt => opt.MapFrom(src => src.Region))
                 .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
                 ;

            cfg.CreateMap<KalturaHouseholdFilter, DomainFilter>()
               .ForMember(dest => dest.ExternalIdEqual, opt => opt.MapFrom(src => src.ExternalIdEqual));
        }

        private static KalturaHouseholdState ConvertDomainStatus(DomainStatus type)
        {
            KalturaHouseholdState result;
            switch (type)
            {
                case DomainStatus.OK:
                    result = KalturaHouseholdState.ok;
                    break;
                case DomainStatus.DomainCreatedWithoutNPVRAccount:
                    result = KalturaHouseholdState.created_without_npvr_account;
                    break;
                case DomainStatus.DomainSuspended:
                    result = KalturaHouseholdState.suspended;
                    break;
                case DomainStatus.NoUsersInDomain:
                    result = KalturaHouseholdState.no_users_in_household;
                    break;
                case DomainStatus.Pending:
                    result = KalturaHouseholdState.pending;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, $"Unknown domain state: {type}");
            }
            return result;
        }

        private static KalturaHouseholdRestriction ConvertDomainRestriction(DomainRestriction type)
        {
            KalturaHouseholdRestriction result;
            switch (type)
            {
                case DomainRestriction.Unrestricted:
                    result = KalturaHouseholdRestriction.not_restricted;
                    break;
                case DomainRestriction.DeviceMasterRestricted:
                    result = KalturaHouseholdRestriction.device_master_restricted;
                    break;
                case DomainRestriction.UserMasterRestricted:
                    result = KalturaHouseholdRestriction.user_master_restricted;
                    break;
                case DomainRestriction.DeviceUserMasterRestricted:
                    result = KalturaHouseholdRestriction.device_user_master_restricted;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown domain_restriction value");
            }
            return result;
        }

        private static KalturaDeviceState? ConvertDeviceState(DeviceState type)
        {
            KalturaDeviceState result;
            switch (type)
            {
                case DeviceState.Activated:
                    result = KalturaDeviceState.activated;
                    break;
                case DeviceState.Pending:
                    result = KalturaDeviceState.pending;
                    break;
                case DeviceState.UnActivated:
                    result = KalturaDeviceState.not_activated;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown device state");
            }
            return result;
        }

        private static KalturaDeviceStatus? ConvertDeviceStatus(DeviceState type)
        {
            KalturaDeviceStatus result;
            switch (type)
            {
                case DeviceState.Activated:
                    result = KalturaDeviceStatus.ACTIVATED;
                    break;
                case DeviceState.Pending:
                    result = KalturaDeviceStatus.PENDING;
                    break;
                case DeviceState.UnActivated:
                    result = KalturaDeviceStatus.NOT_ACTIVATED;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown device state");
            }
            return result;
        }

        public static KalturaDeviceRegistrationStatus ConvertRegistrationStatus(DeviceRegistrationStatus status)
        {
            KalturaDeviceRegistrationStatus result;
            switch (status)
            {
                case DeviceRegistrationStatus.NotRegistered:
                    result = KalturaDeviceRegistrationStatus.not_registered;
                    break;
                case DeviceRegistrationStatus.Registered:
                    result = KalturaDeviceRegistrationStatus.registered;
                    break;
                case DeviceRegistrationStatus.RegisteredToAnotherDomain:
                    result = KalturaDeviceRegistrationStatus.registered_to_another_household;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown device registration status");
            }
            return result;
        }

        public static int ConvertKalturaHouseholdFrequency(KalturaHouseholdFrequencyType householdFrequencyType)
        {
            int result;

            switch (householdFrequencyType)
            {
                case KalturaHouseholdFrequencyType.devices:
                    result = 1;
                    break;
                case KalturaHouseholdFrequencyType.users:
                    result = 2;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown household frequency type");
            }

            return result;
        }
    }
}