using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Api;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Mapping.Utils;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class ApiMappings
    {
        public static void RegisterMappings()
        {
            //Language 
            Mapper.CreateMap<LanguageObj, Language>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault));

            //AssetType to Catalog.StatsType
            Mapper.CreateMap<AssetType, WebAPI.Catalog.StatsType>().ConstructUsing((AssetType type) =>
            {
                WebAPI.Catalog.StatsType result;
                switch (type)
                {
                    case AssetType.media:
                        result = WebAPI.Catalog.StatsType.MEDIA;
                        break;
                    case AssetType.epg:
                        result = WebAPI.Catalog.StatsType.EPG;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown asset type");
                }
                return result;
            });

            #region Parental Rules

            // ParentalRule
            Mapper.CreateMap<WebAPI.Api.ParentalRule, WebAPI.Models.API.KalturaParentalRule>()
                .ForMember(dest => dest.blockAnonymousAccess, opt => opt.MapFrom(src => src.blockAnonymousAccess))
                .ForMember(dest => dest.description, opt => opt.MapFrom(src => src.description))
                .ForMember(dest => dest.epgTagTypeId, opt => opt.MapFrom(src => src.epgTagTypeId))
                .ForMember(dest => dest.epgTagValues, opt => opt.MapFrom(src => src.epgTagValues))
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.isDefault, opt => opt.MapFrom(src => src.isDefault))
                .ForMember(dest => dest.mediaTagTypeId, opt => opt.MapFrom(src => src.mediaTagTypeId))
                .ForMember(dest => dest.mediaTagValues, opt => opt.MapFrom(src => src.mediaTagValues))
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.order, opt => opt.MapFrom(src => src.order))
                .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => ConvertRuleLevel(src.level)))
                .ForMember(dest => dest.ruleType, opt => opt.MapFrom(src => ConvertParentalRuleType(src.ruleType)));

            // PinResponse
            Mapper.CreateMap<WebAPI.Api.PinResponse, WebAPI.Models.API.KalturaPinResponse>()
                .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => ConvertRuleLevel(src.level)))
                .ForMember(dest => dest.PIN, opt => opt.MapFrom(src => src.pin))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaPinType.parental));

            // Pin
            Mapper.CreateMap<WebAPI.Api.PinResponse, WebAPI.Models.API.KalturaPin>()
                .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => ConvertRuleLevel(src.level)))
                .ForMember(dest => dest.PIN, opt => opt.MapFrom(src => src.pin))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaPinType.parental));

            // Purchase Settings
            Mapper.CreateMap<WebAPI.Api.PurchaseSettingsResponse, WebAPI.Models.API.KalturaPurchaseSettings>()
                .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => ConvertRuleLevel(src.level)))
                .ForMember(dest => dest.PIN, opt => opt.MapFrom(src => src.pin))
                .ForMember(dest => dest.Permission, opt => opt.MapFrom(src => ConvertPurchaseSetting(src.type)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaPinType.purchase));

            // Purchase Settings Response
            Mapper.CreateMap<WebAPI.Api.PurchaseSettingsResponse, WebAPI.Models.API.KalturaPurchaseSettingsResponse>()
                .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => ConvertRuleLevel(src.level)))
                .ForMember(dest => dest.PIN, opt => opt.MapFrom(src => src.pin))
                .ForMember(dest => dest.PurchaseSettingsType, opt => opt.MapFrom(src => ConvertPurchaseSetting(src.type)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaPinType.purchase));

            Mapper.CreateMap<WebAPI.Api.GenericRule, WebAPI.Models.API.KalturaGenericRule>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.RuleType, opt => opt.MapFrom(src => ConvertRuleType(src.RuleType)));

            //KalturaUserAssetRule
            Mapper.CreateMap<WebAPI.Api.GenericRule, KalturaUserAssetRule>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.RuleType, opt => opt.MapFrom(src => ConvertRuleType(src.RuleType)));
            #endregion

            #region OSS Adapter

            Mapper.CreateMap<WebAPI.Models.API.KalturaOSSAdapterProfile, OSSAdapter>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.SkipSettings, opt => opt.MapFrom(src => src.Settings == null))
               .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertOSSAdapterSettings(src.Settings)))
               .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier));

            Mapper.CreateMap<OSSAdapter, WebAPI.Models.API.KalturaOSSAdapterProfile>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
              .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertOSSAdapterSettings(src.Settings)))
              .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier));

            Mapper.CreateMap<OSSAdapterBase, WebAPI.Models.API.KalturaOSSAdapterBaseProfile>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            Mapper.CreateMap<OSSAdapterResponse, WebAPI.Models.API.KalturaOSSAdapterProfile>()
             .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.OSSAdapter.AdapterUrl))
             .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.OSSAdapter.ExternalIdentifier))
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OSSAdapter.ID))
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.OSSAdapter.IsActive))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.OSSAdapter.Name))
             .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.OSSAdapter.SharedSecret))
             .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertOSSAdapterSettings(src.OSSAdapter.Settings)));

            #endregion

            #region Recommendation Engine

            Mapper.CreateMap<WebAPI.Models.API.KalturaRecommendationProfile, RecommendationEngine>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.SkipSettings, opt => opt.MapFrom(src => src.Settings == null))
               .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertRecommendationEngineSettings(src.Settings)))
               .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier));

            Mapper.CreateMap<RecommendationEngine, WebAPI.Models.API.KalturaRecommendationProfile>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
              .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertRecommendationEngineSettings(src.Settings)))
              .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier));

            Mapper.CreateMap<RecommendationEngineResponse, WebAPI.Models.API.KalturaRecommendationProfile>()
             .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.RecommendationEngine.AdapterUrl))
             .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.RecommendationEngine.ExternalIdentifier))
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.RecommendationEngine.ID))
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.RecommendationEngine.IsActive))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.RecommendationEngine.Name))
             .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.RecommendationEngine.SharedSecret))
             .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertRecommendationEngineSettings(src.RecommendationEngine.Settings)));

            #endregion

            #region External Channel

            Mapper.CreateMap<WebAPI.Models.API.KalturaExternalChannelProfile, ExternalChannel>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier))
               .ForMember(dest => dest.RecommendationEngineId, opt => opt.MapFrom(src => src.RecommendationEngineId))
               .ForMember(dest => dest.FilterExpression, opt => opt.MapFrom(src => src.FilterExpression))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.Enrichments, opt => opt.MapFrom(src => ConvertEnrichments(src.Enrichments)))
               ;

            Mapper.CreateMap<ExternalChannel, WebAPI.Models.API.KalturaExternalChannelProfile>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier))
               .ForMember(dest => dest.RecommendationEngineId, opt => opt.MapFrom(src => src.RecommendationEngineId))
               .ForMember(dest => dest.FilterExpression, opt => opt.MapFrom(src => src.FilterExpression))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.Enrichments, opt => opt.MapFrom(src => ConvertEnrichments(src.Enrichments)))
               ;

            Mapper.CreateMap<ExternalChannelResponse, WebAPI.Models.API.KalturaExternalChannelProfile>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ExternalChannel.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ExternalChannel.Name))
               .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalChannel.ExternalIdentifier))
               .ForMember(dest => dest.RecommendationEngineId, opt => opt.MapFrom(src => src.ExternalChannel.RecommendationEngineId))
               .ForMember(dest => dest.FilterExpression, opt => opt.MapFrom(src => src.ExternalChannel.FilterExpression))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.ExternalChannel.IsActive))
               .ForMember(dest => dest.Enrichments, opt => opt.MapFrom(src => ConvertEnrichments(src.ExternalChannel.Enrichments)))
               ;

            #endregion

            #region Export Tasks

            //Bulk export task 
            Mapper.CreateMap<BulkExportTask, KalturaExportTask>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.ExternalKey))
                .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => ConvertExportDataType(src.DataType)))
                .ForMember(dest => dest.ExportType, opt => opt.MapFrom(src => ConvertExportType(src.ExportType)))
                .ForMember(dest => dest.Filter, opt => opt.MapFrom(src => src.Filter))
                .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.Frequency))
                .ForMember(dest => dest.NotificationUrl, opt => opt.MapFrom(src => src.NotificationUrl))
                .ForMember(dest => dest.VodTypes, opt => opt.MapFrom(src => src.VodTypes))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
            #endregion

            #region Roles and Permissions

            Mapper.CreateMap<PermissionItem, KalturaPermissionItem>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            Mapper.CreateMap<ApiActionPermissionItem, KalturaApiActionPermissionItem>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service))
               .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action));

            Mapper.CreateMap<ApiParameterPermissionItem, KalturaApiParameterPermissionItem>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Object, opt => opt.MapFrom(src => src.Object))
               .ForMember(dest => dest.Parameter, opt => opt.MapFrom(src => src.Parameter))
               .ForMember(dest => dest.Action, opt => opt.MapFrom(src => ConvertApiParameterPermissionItemAction(src.Action)));

            Mapper.CreateMap<ApiArgumentPermissionItem, KalturaApiArgumentPermissionItem>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service))
               .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
               .ForMember(dest => dest.Parameter, opt => opt.MapFrom(src => src.Parameter));

            Mapper.CreateMap<Permission, KalturaPermission>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.PermissionItems, opt => opt.MapFrom(src => ConvertPermissionItems(src.PermissionItems)));

            Mapper.CreateMap<GroupPermission, KalturaGroupPermission>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.PermissionItems, opt => opt.MapFrom(src => ConvertPermissionItems(src.PermissionItems)))
              .ForMember(dest => dest.Group, opt => opt.MapFrom(src => src.UsersGroup));

            Mapper.CreateMap<Role, KalturaUserRole>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => ConvertPermissions(src.Permissions)));

            #endregion

            #region KSQL Channel
            Mapper.CreateMap<WebAPI.Models.API.KalturaChannelProfile, KSQLChannel>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AssetTypes, opt => opt.MapFrom(src => src.AssetTypes))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
               .ForMember(dest => dest.FilterQuery, opt => opt.MapFrom(src => src.FilterExpression))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Convert.ToInt32(src.IsActive)))
               .ForMember(dest => dest.Order, opt => opt.MapFrom(src => ApiMappings.ConvertOrderToOrderObj(src.Order)))
               ;

            Mapper.CreateMap<KSQLChannel, WebAPI.Models.API.KalturaChannelProfile>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AssetTypes, opt => opt.MapFrom(src => src.AssetTypes))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
               .ForMember(dest => dest.FilterExpression, opt => opt.MapFrom(src => src.FilterQuery))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Convert.ToBoolean(src.IsActive)))
               .ForMember(dest => dest.Order, opt => opt.MapFrom(src => ApiMappings.ConvertOrderObjToOrder(src.Order)))
               ;

            Mapper.CreateMap<KalturaChannel, KSQLChannel>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AssetTypes, opt => opt.MapFrom(src => src.getAssetTypes()))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
               .ForMember(dest => dest.FilterQuery, opt => opt.MapFrom(src => src.FilterExpression))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Convert.ToInt32(src.IsActive)))
               .ForMember(dest => dest.Order, opt => opt.MapFrom(src => ApiMappings.ConvertAssetOrderToOrderObj(src.Order)))
               ;

            Mapper.CreateMap<KSQLChannel, KalturaChannel>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AssetTypes, opt => opt.MapFrom(src => src.AssetTypes))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
               .ForMember(dest => dest.FilterExpression, opt => opt.MapFrom(src => src.FilterQuery))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Convert.ToBoolean(src.IsActive)))
               .ForMember(dest => dest.Order, opt => opt.MapFrom(src => ApiMappings.ConvertOrderObjToAssetOrder(src.Order)))
               ;

            #endregion

            //Api.RegistrySettings to KalturaRegistrySettings
            Mapper.CreateMap<RegistrySettings, KalturaRegistrySettings>()
              .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.key))
              .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.value));

            #region TimeShiftedTv

            //TimeShiftedTvPartnerSettings to KalturaTimeShiftedTvPartnerSettings
            Mapper.CreateMap<TimeShiftedTvPartnerSettings, WebAPI.Models.API.KalturaTimeShiftedTvPartnerSettings>()
                .ForMember(dest => dest.CatchUpEnabled, opt => opt.MapFrom(src => src.IsCatchUpEnabled))
                .ForMember(dest => dest.CdvrEnabled, opt => opt.MapFrom(src => src.IsCdvrEnabled))
                .ForMember(dest => dest.StartOverEnabled, opt => opt.MapFrom(src => src.IsStartOverEnabled))
                .ForMember(dest => dest.TrickPlayEnabled, opt => opt.MapFrom(src => src.IsTrickPlayEnabled))
                .ForMember(dest => dest.CatchUpBufferLength, opt => opt.MapFrom(src => src.CatchUpBufferLength))
                .ForMember(dest => dest.TrickPlayBufferLength, opt => opt.MapFrom(src => src.TrickPlayBufferLength))
                .ForMember(dest => dest.RecordingScheduleWindow, opt => opt.MapFrom(src => src.RecordingScheduleWindow))
                .ForMember(dest => dest.RecordingScheduleWindowEnabled, opt => opt.MapFrom(src => src.IsRecordingScheduleWindowEnabled))
                .ForMember(dest => dest.PaddingBeforeProgramStarts, opt => opt.MapFrom(src => src.PaddingBeforeProgramStarts))
                .ForMember(dest => dest.PaddingAfterProgramEnds, opt => opt.MapFrom(src => src.PaddingAfterProgramEnds))
                .ForMember(dest => dest.ProtectionEnabled, opt => opt.MapFrom(src => src.IsProtectionEnabled))
                .ForMember(dest => dest.ProtectionPeriod, opt => opt.MapFrom(src => src.ProtectionPeriod))
                .ForMember(dest => dest.ProtectionQuotaPercentage, opt => opt.MapFrom(src => src.ProtectionQuotaPercentage))
                .ForMember(dest => dest.RecordingLifetimePeriod, opt => opt.MapFrom(src => src.RecordingLifetimePeriod))
                .ForMember(dest => dest.CleanupNoticePeriod, opt => opt.MapFrom(src => src.CleanupNoticePeriod))
                .ForMember(dest => dest.SeriesRecordingEnabled, opt => opt.MapFrom(src => src.IsSeriesRecordingEnabled))
                .ForMember(dest => dest.NonEntitledChannelPlaybackEnabled, opt => opt.MapFrom(src => src.IsRecordingPlaybackNonEntitledChannelEnabled))
                .ForMember(dest => dest.NonExistingChannelPlaybackEnabled, opt => opt.MapFrom(src => src.IsRecordingPlaybackNonExistingChannelEnabled));

            //KalturaTimeShiftedTvPartnerSettings to TimeShiftedTvPartnerSettings
            Mapper.CreateMap<WebAPI.Models.API.KalturaTimeShiftedTvPartnerSettings, TimeShiftedTvPartnerSettings>()
                .ForMember(dest => dest.IsCatchUpEnabled, opt => opt.MapFrom(src => src.CatchUpEnabled))
                .ForMember(dest => dest.IsCdvrEnabled, opt => opt.MapFrom(src => src.CdvrEnabled))
                .ForMember(dest => dest.IsStartOverEnabled, opt => opt.MapFrom(src => src.StartOverEnabled))
                .ForMember(dest => dest.IsTrickPlayEnabled, opt => opt.MapFrom(src => src.TrickPlayEnabled))
                .ForMember(dest => dest.CatchUpBufferLength, opt => opt.MapFrom(src => src.CatchUpBufferLength))
                .ForMember(dest => dest.TrickPlayBufferLength, opt => opt.MapFrom(src => src.TrickPlayBufferLength))
                .ForMember(dest => dest.RecordingScheduleWindow, opt => opt.MapFrom(src => src.RecordingScheduleWindow))
                .ForMember(dest => dest.IsRecordingScheduleWindowEnabled, opt => opt.MapFrom(src => src.RecordingScheduleWindowEnabled))
                .ForMember(dest => dest.PaddingBeforeProgramStarts, opt => opt.MapFrom(src => src.PaddingBeforeProgramStarts))
                .ForMember(dest => dest.PaddingAfterProgramEnds, opt => opt.MapFrom(src => src.PaddingAfterProgramEnds))
                .ForMember(dest => dest.IsProtectionEnabled, opt => opt.MapFrom(src => src.ProtectionEnabled))
                .ForMember(dest => dest.ProtectionPeriod, opt => opt.MapFrom(src => src.ProtectionPeriod))
                .ForMember(dest => dest.ProtectionQuotaPercentage, opt => opt.MapFrom(src => src.ProtectionQuotaPercentage))
                .ForMember(dest => dest.RecordingLifetimePeriod, opt => opt.MapFrom(src => src.RecordingLifetimePeriod))
                .ForMember(dest => dest.CleanupNoticePeriod, opt => opt.MapFrom(src => src.CleanupNoticePeriod))
                .ForMember(dest => dest.IsSeriesRecordingEnabled, opt => opt.MapFrom(src => src.SeriesRecordingEnabled))
                .ForMember(dest => dest.IsRecordingPlaybackNonEntitledChannelEnabled, opt => opt.MapFrom(src => src.NonEntitledChannelPlaybackEnabled))
                .ForMember(dest => dest.IsRecordingPlaybackNonExistingChannelEnabled, opt => opt.MapFrom(src => src.NonExistingChannelPlaybackEnabled));

            #endregion

            #region CDN Adapter

            Mapper.CreateMap<KalturaCDNAdapterProfile, WebAPI.Api.CDNAdapter>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
               .ForMember(dest => dest.BaseUrl, opt => opt.MapFrom(src => src.BaseUrl))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive.HasValue ? src.IsActive.Value : true))
               .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertCDNAdapterSettings(src.Settings)))
               .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
               .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret));

            Mapper.CreateMap<WebAPI.Api.CDNAdapter, KalturaCDNAdapterProfile>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
              .ForMember(dest => dest.BaseUrl, opt => opt.MapFrom(src => src.BaseUrl))
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
              .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertCDNAdapterSettings(src.Settings)))
              .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
              .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret));

            #endregion

            #region CDN Settings

            //CDNPartnerSettings to KalturaCDNPartnerSettings
            Mapper.CreateMap<CDNPartnerSettings, KalturaCDNPartnerSettings>()
                .ForMember(dest => dest.DefaultRecordingAdapterId, opt => opt.MapFrom(src => src.DefaultRecordingAdapter))
                .ForMember(dest => dest.DefaultAdapterId, opt => opt.MapFrom(src => src.DefaultAdapter));

            //KalturaCDNPartnerSettings to CDNPartnerSettings 
            Mapper.CreateMap<KalturaCDNPartnerSettings, CDNPartnerSettings>()
                .ForMember(dest => dest.DefaultRecordingAdapter, opt => opt.MapFrom(src => src.DefaultRecordingAdapterId))
                .ForMember(dest => dest.DefaultAdapter, opt => opt.MapFrom(src => src.DefaultAdapterId));

            #endregion

            #region regions
            Mapper.CreateMap<KeyValuePair, KalturaRegionalChannel>()
              .ForMember(dest => dest.LinearChannelId, opt => opt.MapFrom(src => src.key))
              .ForMember(dest => dest.ChannelNumber, opt => opt.MapFrom(src => src.value));


            Mapper.CreateMap<Region, KalturaRegion>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
              .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.externalId))
              .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.isDefault))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
              .ForMember(dest => dest.RegionalChannels, opt => opt.MapFrom(src => src.linearChannels));

            #endregion

            #region DeviceFamily

            //TimeShiftedTvPartnerSettings to KalturaTimeShiftedTvPartnerSettings
            Mapper.CreateMap<DeviceFamily, WebAPI.Models.Domains.KalturaDeviceFamily>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            Mapper.CreateMap<DeviceBrand, WebAPI.Models.Domains.KalturaDeviceBrand>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DeviceFamilyId, opt => opt.MapFrom(src => src.DeviceFamilyId));

            #endregion

            #region DeviceFamily
            Mapper.CreateMap<Country, WebAPI.Models.Users.KalturaCountry>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code));
            #endregion

            #region Meta

            Mapper.CreateMap<Meta, KalturaMeta>()
              .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => ConvertAssetType(src.AssetType)))
              .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => ConvertFieldName(src.FieldName)))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertMetaType(src.Type)))
              ;

            Mapper.CreateMap<KalturaMeta, Meta>()
             .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => ConvertAssetType(src.AssetType)))
             .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => ConvertMetaFieldName(src.FieldName)))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertMetaType(src.Type)))
             ;

            #endregion
        }

        private static KalturaApiParameterPermissionItemAction ConvertApiParameterPermissionItemAction(string action)
        {
            return (KalturaApiParameterPermissionItemAction)Enum.Parse(typeof(KalturaApiParameterPermissionItemAction), action);
        }

        private static List<KalturaPermissionItem> ConvertPermissionItems(PermissionItem[] permissionItems)
        {
            List<KalturaPermissionItem> result = null;

            if (permissionItems != null && permissionItems.Length > 0)
            {
                result = new List<KalturaPermissionItem>();

                KalturaPermissionItem item;

                foreach (var permissionItem in permissionItems)
                {
                    if (permissionItem is ApiActionPermissionItem)
                    {
                        item = AutoMapper.Mapper.Map<KalturaApiActionPermissionItem>((ApiActionPermissionItem)permissionItem);
                    }
                    else if (permissionItem is ApiParameterPermissionItem)
                    {
                        item = AutoMapper.Mapper.Map<KalturaApiParameterPermissionItem>((ApiParameterPermissionItem)permissionItem);
                    }
                    else if (permissionItem is ApiArgumentPermissionItem)
                    {
                        item = AutoMapper.Mapper.Map<KalturaApiArgumentPermissionItem>((ApiArgumentPermissionItem)permissionItem);
                    }
                    else
                    {
                        item = AutoMapper.Mapper.Map<KalturaPermissionItem>(permissionItem);
                    }
                    result.Add(item);
                }
            }

            return result;
        }

        private static List<KalturaPermission> ConvertPermissions(Permission[] permissions)
        {
            List<KalturaPermission> result = null;

            if (permissions != null && permissions.Length > 0)
            {
                result = new List<KalturaPermission>();

                KalturaPermission kalturaPermission;

                foreach (var permission in permissions)
                {
                    if (permission is GroupPermission)
                    {
                        kalturaPermission = AutoMapper.Mapper.Map<KalturaGroupPermission>((GroupPermission)permission);
                    }
                    else
                    {
                        kalturaPermission = AutoMapper.Mapper.Map<KalturaPermission>(permission);
                    }

                    result.Add(kalturaPermission);
                }
            }

            return result;
        }

        private static List<KalturaChannelEnrichmentHolder> ConvertEnrichments(ExternalRecommendationEngineEnrichment[] list)
        {
            List<KalturaChannelEnrichmentHolder> result = null;

            if (list != null && list.Length > 0)
            {
                result = new List<KalturaChannelEnrichmentHolder>();

                foreach (ExternalRecommendationEngineEnrichment ExternalRecommendationEngineEnrichment in list)
                {
                    result.Add(ConvertChannelEnrichment(ExternalRecommendationEngineEnrichment));
                }
            }

            return result;
        }

        private static KalturaChannelEnrichmentHolder ConvertChannelEnrichment(ExternalRecommendationEngineEnrichment type)
        {
            KalturaChannelEnrichmentHolder result = null;

            switch (type)
            {
                case ExternalRecommendationEngineEnrichment.AtHome:
                    result = new KalturaChannelEnrichmentHolder() { type = KalturaChannelEnrichment.AtHome };
                    break;
                case ExternalRecommendationEngineEnrichment.Catchup:
                    result = new KalturaChannelEnrichmentHolder() { type = KalturaChannelEnrichment.Catchup };
                    break;
                case ExternalRecommendationEngineEnrichment.ClientLocation:
                    result = new KalturaChannelEnrichmentHolder() { type = KalturaChannelEnrichment.ClientLocation };
                    break;
                case ExternalRecommendationEngineEnrichment.DeviceId:
                    result = new KalturaChannelEnrichmentHolder() { type = KalturaChannelEnrichment.DeviceId };
                    break;
                case ExternalRecommendationEngineEnrichment.DeviceType:
                    result = new KalturaChannelEnrichmentHolder() { type = KalturaChannelEnrichment.DeviceType };
                    break;
                case ExternalRecommendationEngineEnrichment.DTTRegion:
                    result = new KalturaChannelEnrichmentHolder() { type = KalturaChannelEnrichment.DTTRegion };
                    break;
                case ExternalRecommendationEngineEnrichment.HouseholdId:
                    result = new KalturaChannelEnrichmentHolder() { type = KalturaChannelEnrichment.HouseholdId };
                    break;
                case ExternalRecommendationEngineEnrichment.Language:
                    result = new KalturaChannelEnrichmentHolder() { type = KalturaChannelEnrichment.Language };
                    break;
                case ExternalRecommendationEngineEnrichment.NPVRSupport:
                    result = new KalturaChannelEnrichmentHolder() { type = KalturaChannelEnrichment.NPVRSupport };
                    break;
                case ExternalRecommendationEngineEnrichment.Parental:
                    result = new KalturaChannelEnrichmentHolder() { type = KalturaChannelEnrichment.Parental };
                    break;
                case ExternalRecommendationEngineEnrichment.UserId:
                    result = new KalturaChannelEnrichmentHolder() { type = KalturaChannelEnrichment.UserId };
                    break;
                case ExternalRecommendationEngineEnrichment.UTCOffset:
                    result = new KalturaChannelEnrichmentHolder() { type = KalturaChannelEnrichment.UTCOffset };
                    break;

                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown external channel enrichment");
            }

            return result;
        }

        private static ExternalRecommendationEngineEnrichment[] ConvertEnrichments(List<KalturaChannelEnrichmentHolder> list)
        {
            List<ExternalRecommendationEngineEnrichment> result = null;

            if (list != null && list.Count > 0)
            {
                result = new List<ExternalRecommendationEngineEnrichment>();
                if (list != null && list.Count > 0)
                {
                    result = new List<ExternalRecommendationEngineEnrichment>();

                    foreach (KalturaChannelEnrichmentHolder kalturaChannelEnrichmentHolder in list)
                    {
                        result.Add(ConvertChannelEnrichment(kalturaChannelEnrichmentHolder.type));
                    }
                }
            }
            if (result != null && result.Count > 0)
            {
                return result.ToArray();
            }
            else
            {
                return null;
            }
        }

        private static ExternalRecommendationEngineEnrichment ConvertChannelEnrichment(KalturaChannelEnrichment type)
        {
            ExternalRecommendationEngineEnrichment result;

            switch (type)
            {
                case KalturaChannelEnrichment.AtHome:
                    result = ExternalRecommendationEngineEnrichment.AtHome;
                    break;
                case KalturaChannelEnrichment.Catchup:
                    result = ExternalRecommendationEngineEnrichment.Catchup;
                    break;
                case KalturaChannelEnrichment.ClientLocation:
                    result = ExternalRecommendationEngineEnrichment.ClientLocation;
                    break;
                case KalturaChannelEnrichment.DeviceId:
                    result = ExternalRecommendationEngineEnrichment.DeviceId;
                    break;
                case KalturaChannelEnrichment.DeviceType:
                    result = ExternalRecommendationEngineEnrichment.DeviceType;
                    break;
                case KalturaChannelEnrichment.DTTRegion:
                    result = ExternalRecommendationEngineEnrichment.DTTRegion;
                    break;
                case KalturaChannelEnrichment.HouseholdId:
                    result = ExternalRecommendationEngineEnrichment.HouseholdId;
                    break;
                case KalturaChannelEnrichment.Language:
                    result = ExternalRecommendationEngineEnrichment.Language;
                    break;
                case KalturaChannelEnrichment.NPVRSupport:
                    result = ExternalRecommendationEngineEnrichment.NPVRSupport;
                    break;
                case KalturaChannelEnrichment.Parental:
                    result = ExternalRecommendationEngineEnrichment.Parental;
                    break;
                case KalturaChannelEnrichment.UserId:
                    result = ExternalRecommendationEngineEnrichment.UserId;
                    break;
                case KalturaChannelEnrichment.UTCOffset:
                    result = ExternalRecommendationEngineEnrichment.UTCOffset;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown external channel enrichment");
            }

            return result;
        }

        internal static Dictionary<string, KalturaStringValue> ConvertRecommendationEngineSettings(RecommendationEngineSettings[] settings)
        {
            if (settings == null)
                return null;

            Dictionary<string, KalturaStringValue> result = null;

            if (settings.Count() > 0)
            {
                result = new Dictionary<string, KalturaStringValue>();
                foreach (var data in settings)
                {
                    if (!string.IsNullOrEmpty(data.key))
                    {
                        result.Add(data.key, new KalturaStringValue() { value = data.value });
                    }
                }
            }
            return result;
        }

        internal static RecommendationEngineSettings[] ConvertRecommendationEngineSettings(SerializableDictionary<string, KalturaStringValue> settings)
        {
            if (settings == null)
                return null;

            List<Api.RecommendationEngineSettings> result = null;

            if (settings.Count > 0)
            {
                result = new List<RecommendationEngineSettings>();
                Api.RecommendationEngineSettings pc;
                foreach (KeyValuePair<string, KalturaStringValue> data in settings)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        pc = new Api.RecommendationEngineSettings();
                        pc.key = data.Key;
                        pc.value = data.Value.value;
                        result.Add(pc);
                    }
                }
            }
            if (result != null && result.Count > 0)
            {
                return result.ToArray();
            }
            else
            {
                return null;
            }
        }

        private static WebAPI.Models.API.KalturaParentalRuleType ConvertParentalRuleType(WebAPI.Api.eParentalRuleType type)
        {
            WebAPI.Models.API.KalturaParentalRuleType result = WebAPI.Models.API.KalturaParentalRuleType.ALL;

            switch (type)
            {
                case WebAPI.Api.eParentalRuleType.All:
                    result = WebAPI.Models.API.KalturaParentalRuleType.ALL;
                    break;
                case WebAPI.Api.eParentalRuleType.Movies:
                    result = WebAPI.Models.API.KalturaParentalRuleType.MOVIES;
                    break;
                case WebAPI.Api.eParentalRuleType.TVSeries:
                    result = WebAPI.Models.API.KalturaParentalRuleType.TV_SERIES;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown asset type");
            }

            return result;
        }

        private static WebAPI.Models.API.KalturaRuleLevel ConvertRuleLevel(WebAPI.Api.eRuleLevel? type)
        {
            WebAPI.Models.API.KalturaRuleLevel result = WebAPI.Models.API.KalturaRuleLevel.invalid;

            switch (type)
            {
                case WebAPI.Api.eRuleLevel.User:
                    result = WebAPI.Models.API.KalturaRuleLevel.user;
                    break;
                case WebAPI.Api.eRuleLevel.Domain:
                    result = WebAPI.Models.API.KalturaRuleLevel.household;
                    break;
                case WebAPI.Api.eRuleLevel.Group:
                    result = WebAPI.Models.API.KalturaRuleLevel.account;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown rule level");

            }

            return result;
        }

        private static WebAPI.Models.API.KalturaPurchaseSettingsType? ConvertPurchaseSetting(WebAPI.Api.ePurchaeSettingsType? type)
        {
            WebAPI.Models.API.KalturaPurchaseSettingsType result = WebAPI.Models.API.KalturaPurchaseSettingsType.block;

            if (type == null)
            {
                return null;
            }

            switch (type)
            {
                case WebAPI.Api.ePurchaeSettingsType.Allow:
                    result = WebAPI.Models.API.KalturaPurchaseSettingsType.allow;
                    break;
                case WebAPI.Api.ePurchaeSettingsType.Ask:
                    result = WebAPI.Models.API.KalturaPurchaseSettingsType.ask;
                    break;
                case WebAPI.Api.ePurchaeSettingsType.Block:
                    result = WebAPI.Models.API.KalturaPurchaseSettingsType.block;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown purchase setting");

            }

            return result;
        }

        private static WebAPI.Models.API.KalturaRuleType ConvertRuleType(WebAPI.Api.RuleType type)
        {
            WebAPI.Models.API.KalturaRuleType result;

            switch (type)
            {
                case RuleType.Parental:
                    result = WebAPI.Models.API.KalturaRuleType.parental;
                    break;
                case RuleType.Geo:
                    result = WebAPI.Models.API.KalturaRuleType.geo;
                    break;
                case RuleType.UserType:
                    result = WebAPI.Models.API.KalturaRuleType.user_type;
                    break;
                case RuleType.Device:
                    result = WebAPI.Models.API.KalturaRuleType.device;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown rule type");
            }

            return result;
        }

        internal static Dictionary<string, int> ConvertErrorsDictionary(KeyValuePair[] errors)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            foreach (var item in errors)
            {
                if (!result.ContainsKey(item.key))
                {
                    result.Add(item.key, int.Parse(item.value));
                }
            }

            return result;
        }

        internal static OSSAdapterSettings[] ConvertOSSAdapterSettings(SerializableDictionary<string, KalturaStringValue> settings)
        {
            List<Api.OSSAdapterSettings> result = null;

            if (settings != null && settings.Count > 0)
            {
                result = new List<OSSAdapterSettings>();
                Api.OSSAdapterSettings pc;
                foreach (KeyValuePair<string, KalturaStringValue> data in settings)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        pc = new Api.OSSAdapterSettings();
                        pc.key = data.Key;
                        pc.value = data.Value.value;
                        result.Add(pc);
                    }
                }
            }
            if (result != null && result.Count > 0)
            {
                return result.ToArray();
            }
            else
            {
                return null;
            }
        }

        public static Dictionary<string, KalturaStringValue> ConvertOSSAdapterSettings(Api.OSSAdapterSettings[] settings)
        {
            Dictionary<string, KalturaStringValue> result = null;

            if (settings != null && settings.Count() > 0)
            {
                result = new Dictionary<string, KalturaStringValue>();
                foreach (var data in settings)
                {
                    if (!string.IsNullOrEmpty(data.key))
                    {
                        result.Add(data.key, new KalturaStringValue() { value = data.value });
                    }
                }
            }
            return result;
        }

        private static WebAPI.Models.API.KalturaExportDataType ConvertExportDataType(WebAPI.Api.eBulkExportDataType type)
        {
            WebAPI.Models.API.KalturaExportDataType result;

            switch (type)
            {
                case eBulkExportDataType.EPG:
                    result = WebAPI.Models.API.KalturaExportDataType.epg;
                    break;
                case eBulkExportDataType.Users:
                    result = WebAPI.Models.API.KalturaExportDataType.users;
                    break;
                case eBulkExportDataType.VOD:
                    result = WebAPI.Models.API.KalturaExportDataType.vod;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown bulk export data type");
            }

            return result;
        }

        private static WebAPI.Models.API.KalturaExportType ConvertExportType(WebAPI.Api.eBulkExportExportType type)
        {
            WebAPI.Models.API.KalturaExportType result;

            switch (type)
            {
                case eBulkExportExportType.Full:
                    result = WebAPI.Models.API.KalturaExportType.full;
                    break;
                case eBulkExportExportType.Incremental:
                    result = WebAPI.Models.API.KalturaExportType.incremental;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown bulk export export type");
            }

            return result;
        }

        public static eBulkExportDataType ConvertExportDataType(KalturaExportDataType type)
        {
            eBulkExportDataType result;

            switch (type)
            {
                case KalturaExportDataType.epg:
                    result = eBulkExportDataType.EPG;
                    break;
                case KalturaExportDataType.users:
                    result = eBulkExportDataType.Users;
                    break;
                case KalturaExportDataType.vod:
                    result = eBulkExportDataType.VOD;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown bulk export data type");
            }

            return result;
        }

        public static eBulkExportExportType ConvertExportType(KalturaExportType type)
        {
            eBulkExportExportType result;

            switch (type)
            {
                case KalturaExportType.full:
                    result = eBulkExportExportType.Full;
                    break;
                case KalturaExportType.incremental:
                    result = eBulkExportExportType.Incremental;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown bulk export export type");
            }

            return result;
        }

        public static OrderObj ConvertOrderToOrderObj(WebAPI.Models.Catalog.KalturaOrder order)
        {
            OrderObj result = new OrderObj();

            switch (order)
            {
                case WebAPI.Models.Catalog.KalturaOrder.a_to_z:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
                case WebAPI.Models.Catalog.KalturaOrder.z_to_a:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case WebAPI.Models.Catalog.KalturaOrder.views:
                    result.m_eOrderBy = OrderBy.VIEWS;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case WebAPI.Models.Catalog.KalturaOrder.ratings:
                    result.m_eOrderBy = OrderBy.RATING;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case WebAPI.Models.Catalog.KalturaOrder.votes:
                    result.m_eOrderBy = OrderBy.VOTES_COUNT;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case WebAPI.Models.Catalog.KalturaOrder.newest:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case WebAPI.Models.Catalog.KalturaOrder.relevancy:
                    result.m_eOrderBy = OrderBy.RELATED;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case WebAPI.Models.Catalog.KalturaOrder.oldest_first:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
            }
            return result;
        }

        public static OrderObj ConvertAssetOrderToOrderObj(KalturaAssetOrderBy order)
        {
            OrderObj result = new OrderObj();

            switch (order)
            {
                case KalturaAssetOrderBy.NAME_ASC:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
                case KalturaAssetOrderBy.NAME_DESC:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.VIEWS_DESC:
                    result.m_eOrderBy = OrderBy.VIEWS;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.RATINGS_DESC:
                    result.m_eOrderBy = OrderBy.RATING;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.VOTES_DESC:
                    result.m_eOrderBy = OrderBy.VOTES_COUNT;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.START_DATE_DESC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.RELEVANCY_DESC:
                    result.m_eOrderBy = OrderBy.RELATED;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.START_DATE_ASC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
                case KalturaAssetOrderBy.LIKES_DESC:
                    result.m_eOrderBy = OrderBy.LIKE_COUNTER;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
            }
            return result;
        }

        public static KalturaOrder ConvertOrderObjToOrder(OrderObj orderObj)
        {
            KalturaOrder result = KalturaOrder.newest;

            if (orderObj == null)
            {
                return result;
            }

            switch (orderObj.m_eOrderBy)
            {
                case OrderBy.VIEWS:
                    {
                        result = KalturaOrder.views;
                        break;
                    }
                case OrderBy.RATING:
                    {
                        result = KalturaOrder.ratings;
                        break;
                    }
                case OrderBy.VOTES_COUNT:
                    {
                        result = KalturaOrder.votes;
                        break;
                    }
                case OrderBy.START_DATE:
                    {
                        if (orderObj.m_eOrderDir == OrderDir.DESC)
                        {
                            result = KalturaOrder.newest;
                        }
                        else
                        {
                            result = KalturaOrder.oldest_first;
                        }
                        break;
                    }
                case OrderBy.NAME:
                    {
                        if (orderObj.m_eOrderDir == OrderDir.ASC)
                        {
                            result = KalturaOrder.a_to_z;
                        }
                        else
                        {
                            result = KalturaOrder.z_to_a;
                        }
                        break;
                    }
                case OrderBy.RELATED:
                    {
                        result = KalturaOrder.relevancy;
                        break;
                    }
                case OrderBy.META:
                case OrderBy.CREATE_DATE:
                case OrderBy.RECOMMENDATION:
                case OrderBy.RANDOM:
                case OrderBy.LIKE_COUNTER:
                case OrderBy.NONE:
                case OrderBy.ID:
                default:
                    break;
            }

            return result;
        }

        public static KalturaAssetOrderBy ConvertOrderObjToAssetOrder(OrderBy OrderBy, OrderDir OrderDir)
        {
            KalturaAssetOrderBy result = KalturaAssetOrderBy.START_DATE_DESC;

            switch (OrderBy)
            {
                case OrderBy.VIEWS:
                    {
                        result = KalturaAssetOrderBy.VIEWS_DESC;
                        break;
                    }
                case OrderBy.RATING:
                    {
                        result = KalturaAssetOrderBy.RATINGS_DESC;
                        break;
                    }
                case OrderBy.VOTES_COUNT:
                    {
                        result = KalturaAssetOrderBy.VOTES_DESC;
                        break;
                    }
                case OrderBy.START_DATE:
                    {
                        if (OrderDir == OrderDir.DESC)
                        {
                            result = KalturaAssetOrderBy.START_DATE_DESC;
                        }
                        else
                        {
                            result = KalturaAssetOrderBy.START_DATE_ASC;
                        }
                        break;
                    }
                case OrderBy.NAME:
                    {
                        if (OrderDir == OrderDir.ASC)
                        {
                            result = KalturaAssetOrderBy.NAME_ASC;
                        }
                        else
                        {
                            result = KalturaAssetOrderBy.NAME_DESC;
                        }
                        break;
                    }
                case OrderBy.RELATED:
                    {
                        result = KalturaAssetOrderBy.RELEVANCY_DESC;
                        break;
                    }
                case OrderBy.META:
                case OrderBy.CREATE_DATE:
                case OrderBy.RECOMMENDATION:
                case OrderBy.RANDOM:
                case OrderBy.LIKE_COUNTER:
                case OrderBy.NONE:
                case OrderBy.ID:
                default:
                    break;
            }

            return result;
        }

        public static KalturaAssetOrderBy ConvertOrderObjToAssetOrder(OrderObj orderObj)
        {
            if (orderObj == null)
            {
                return KalturaAssetOrderBy.START_DATE_DESC;
            }

            return ConvertOrderObjToAssetOrder(orderObj.m_eOrderBy, orderObj.m_eOrderDir);
        }

        public static WebAPI.Api.CDNAdapterSettings[] ConvertCDNAdapterSettings(SerializableDictionary<string, KalturaStringValue> settings)
        {
            List<WebAPI.Api.CDNAdapterSettings> result = null;

            if (settings != null && settings.Count > 0)
            {
                result = new List<WebAPI.Api.CDNAdapterSettings>();
                WebAPI.Api.CDNAdapterSettings pc;
                foreach (KeyValuePair<string, KalturaStringValue> data in settings)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        pc = new WebAPI.Api.CDNAdapterSettings();
                        pc.key = data.Key;
                        pc.value = data.Value.value;
                        result.Add(pc);
                    }
                }
            }
            if (result != null && result.Count > 0)
            {
                return result.ToArray();
            }
            else
            {
                return null;
            }
        }

        public static Dictionary<string, KalturaStringValue> ConvertCDNAdapterSettings(WebAPI.Api.CDNAdapterSettings[] settings)
        {
            Dictionary<string, KalturaStringValue> result = null;

            if (settings != null && settings.Count() > 0)
            {
                result = new Dictionary<string, KalturaStringValue>();
                foreach (var data in settings)
                {
                    if (!string.IsNullOrEmpty(data.key))
                    {
                        result.Add(data.key, new KalturaStringValue() { value = data.value });
                    }
                }
            }
            return result;
        }

        public static BulkExportTaskOrderBy ConvertExportTaskOrderBy(KalturaExportTaskOrderBy orderBy)
        {
            BulkExportTaskOrderBy result;

            switch (orderBy)
            {
                case KalturaExportTaskOrderBy.CREATE_DATE_DESC:
                    result = BulkExportTaskOrderBy.CreateDateDesc;
                    break;
                case KalturaExportTaskOrderBy.CREATE_DATE_ASC:
                    result = BulkExportTaskOrderBy.CreateDateAsc;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown export task order by");
            }

            return result;
        }

        public static GenericRuleOrderBy ConvertUserAssetRuleOrderBy(KalturaUserAssetRuleOrderBy orderBy)
        {
            GenericRuleOrderBy result;

            switch (orderBy)
            {
                case KalturaUserAssetRuleOrderBy.NAME_ASC:
                    result = GenericRuleOrderBy.NameAsc;
                    break;
                case KalturaUserAssetRuleOrderBy.NAME_DESC:
                    result = GenericRuleOrderBy.NameAsc;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown user asset rule order by");
            }

            return result;
        }

        internal static RegionOrderBy ConvertRegionOrderBy(KalturaRegionOrderBy orderBy)
        {
            RegionOrderBy result;

            switch (orderBy)
            {
                case KalturaRegionOrderBy.CREATE_DATE_ASC:
                    result = RegionOrderBy.CreateDateAsc;
                    break;
                case KalturaRegionOrderBy.CREATE_DATE_DESC:
                    result = RegionOrderBy.CreateDateDesc;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown region order by");
            }

            return result;
        }

        private static KalturaAssetType ConvertAssetType(eAssetTypes assetType)
        {
            KalturaAssetType response;
            switch (assetType)
            {
                case eAssetTypes.EPG:
                    response = KalturaAssetType.epg;
                    break;
                case eAssetTypes.NPVR:
                    response = KalturaAssetType.recording;
                    break;
                case eAssetTypes.MEDIA:
                    response = KalturaAssetType.media;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown Asset Type");
            }

            return response;
        }

        private static KalturaMetaType ConvertMetaType(MetaType metaType)
        {
            KalturaMetaType response;

            switch (metaType)
            {
                case MetaType.String:
                    response = KalturaMetaType.STRING;
                    break;
                case MetaType.Number:
                    response = KalturaMetaType.NUMBER;
                    break;
                case MetaType.Bool:
                    response = KalturaMetaType.BOOLEAN;
                    break;
                case MetaType.Tag:
                    response = KalturaMetaType.STRING_ARRAY;
                    break;
                case MetaType.All:
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown meta type");
            }

            return response;
        }

        private static KalturaMetaFieldName ConvertFieldName(MetaFieldName metaFieldName)
        {
            KalturaMetaFieldName response;

            switch (metaFieldName)
            {
                case MetaFieldName.None:
                    response = KalturaMetaFieldName.NONE;
                    break;
                case MetaFieldName.SeriesId:
                    response = KalturaMetaFieldName.SERIES_ID;
                    break;
                case MetaFieldName.SeasonNumber:
                    response = KalturaMetaFieldName.SEASON_NUMBER;
                    break;
                case MetaFieldName.EpisodeNumber:
                    response = KalturaMetaFieldName.EPISODE_NUMBER;
                    break;
                case MetaFieldName.All:
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown meta field name type");
            }
            return response;
        }

        internal static eAssetTypes ConvertAssetType(KalturaAssetType? assetType)
        {
            eAssetTypes response = eAssetTypes.UNKNOWN;
            if (assetType.HasValue)
            {
                switch (assetType)
                {
                    case KalturaAssetType.epg:
                        response = eAssetTypes.EPG;
                        break;
                    case KalturaAssetType.recording:
                        response = eAssetTypes.NPVR;
                        break;
                    case KalturaAssetType.media:
                        response = eAssetTypes.MEDIA;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown Asset Type");
                }
            }
            return response;
        }
        
        internal static MetaType ConvertMetaType(KalturaMetaType? metaType)
        {
            MetaType response;

            if (metaType.HasValue)
            {
                switch (metaType.Value)
                {
                    case KalturaMetaType.STRING:
                        response = MetaType.String;
                        break;
                    case KalturaMetaType.NUMBER:
                        response = MetaType.Number;
                        break;
                    case KalturaMetaType.BOOLEAN:
                        response = MetaType.Bool;
                        break;
                    case KalturaMetaType.STRING_ARRAY:
                        response = MetaType.Tag;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown meta type");
                }
            }
            else
            {
                response = MetaType.All;
            }
            return response;
        }
        
        internal static MetaFieldName ConvertMetaFieldName(KalturaMetaFieldName? metaFieldName)
        {
            MetaFieldName response;

            if (metaFieldName.HasValue)
            {
                switch (metaFieldName.Value)
                {
                    case KalturaMetaFieldName.NONE:
                        response = MetaFieldName.None;
                        break;
                    case KalturaMetaFieldName.SERIES_ID:
                        response = MetaFieldName.SeriesId;
                        break;
                    case KalturaMetaFieldName.SEASON_NUMBER:
                        response = MetaFieldName.SeasonNumber;
                        break;
                    case KalturaMetaFieldName.EPISODE_NUMBER:
                        response = MetaFieldName.EpisodeNumber;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown meta field name type");
                }
            }
            else
            {
                response = MetaFieldName.All;
            }

            return response;
        }
    }
}