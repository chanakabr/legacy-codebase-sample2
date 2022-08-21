using System;
using System.Reflection;
using ApiLogic.Api.Managers;
using ApiObjects;
using Core.Catalog.CatalogManagement;
using Core.Notification;
using DAL;
using GrpcAPI.Utils;
using Microsoft.Extensions.Logging;
using phoenix;
using Phx.Lib.Log;

namespace GrpcAPI.Services
{
    public interface IGroupAndConfigurationService
    {
        GetGroupSecretAndCountryCodeResponse GetGroupSecretAndCountryCode(GetGroupSecretAndCountryCodeRequest request);
        GetCDVRAdapterResponse GetCDVRAdapter(GetCDVRAdapterRequest request);
        bool IsRegionalization(IsRegionalizationRequest request);
        GetDefaultRegionIdResponse GetDefaultRegionId(GetDefaultRegionIdRequest request);

        GetNotificationPartnerSettingsResponse GetNotificationPartnerSettings(
            GetNotificationPartnerSettingsRequest request);
    }

    public class GroupAndConfigurationService : IGroupAndConfigurationService
    {
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.ToString());

        public GetGroupSecretAndCountryCodeResponse GetGroupSecretAndCountryCode(
            GetGroupSecretAndCountryCodeRequest request)
        {
            string groupSecretCode = String.Empty;
            string groupCountryCode = String.Empty;
            bool isSuccess =
                DAL.ConditionalAccessDAL.Get_GroupSecretAndCountryCode(request.GroupId, ref groupSecretCode,
                    ref groupCountryCode);
            return new GetGroupSecretAndCountryCodeResponse
            {
                IsSuccess = isSuccess,
                CountryCode = groupCountryCode,
                SecretCode = groupSecretCode
            };
        }

        public GetCDVRAdapterResponse GetCDVRAdapter(GetCDVRAdapterRequest request)
        {
            try
            {
                int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(request.GroupId);
                CDVRAdapter cdvrAdapter = ConditionalAccessDAL.GetCDVRAdapter(request.GroupId, adapterId);
                return new GetCDVRAdapterResponse
                {
                    Id = cdvrAdapter.ID,
                    AdapterUrl = cdvrAdapter.AdapterUrl,
                    DynamicLinksSupport = cdvrAdapter.DynamicLinksSupport,
                    SharedSecret = cdvrAdapter.SharedSecret,
                    ExternalIdentifier = cdvrAdapter.ExternalIdentifier
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while calling GetCDVRAdapter GRPC service {e.Message}");
            }

            return null;
        }
        
        public bool IsRegionalization(IsRegionalizationRequest request)
        {
            return CatalogManager.Instance.IsRegionalizationEnabled(request.GroupId);
        }
        
        public GetDefaultRegionIdResponse GetDefaultRegionId(GetDefaultRegionIdRequest request)
        {
            var regionId = RegionManager.Instance.GetDefaultRegionId(request.GroupId);
            return regionId != null
                ? new GetDefaultRegionIdResponse
                {
                    RegionId = (long) regionId
                }
                : new GetDefaultRegionIdResponse
                {
                    RegionId = -1
                };
        }

        public GetNotificationPartnerSettingsResponse GetNotificationPartnerSettings(
            GetNotificationPartnerSettingsRequest request)
        {
            var notificationSettings = NotificationCache.Instance().GetPartnerNotificationSettings(request.GroupId);
            return new GetNotificationPartnerSettingsResponse{Setting = NotificationPartnerSettings.Parser.ParseFrom(GrpcSerialize.ProtoSerialize(notificationSettings.settings))};
        }
    }
}