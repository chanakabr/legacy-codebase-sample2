using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.BulkExport;
using ApiObjects.CDNAdapter;
using ApiObjects.Notification;
using ApiObjects.Roles;
using ApiObjects.Rules;
using ApiObjects.Rules.FilterActions;
using ApiObjects.SearchObjects;
using ApiObjects.TimeShiftedTv;
using AutoMapper.Configuration;
using Core.Api.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.ConditionalAccess.FilterActions.Assets;
using WebAPI.Models.ConditionalAccess.FilterActions.Files;
using WebAPI.Models.General;
using WebAPI.Models.Users.UserSessionProfile;
using WebAPI.ModelsFactory;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.ObjectsConvertor.Mapping.Utils;
using KeyValuePair = ApiObjects.KeyValuePair;
using WebAPI.ModelsValidators;
using ApiObjects.BulkUpload;
using ApiObjects.Rules.PreActionCondition;
using AutoMapper;
using WebAPI.Models.ConditionalAccess.FilterActions;
using WebAPI.Models.Domains;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class ApiMappings
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            cfg.CreateMap<KalturaFilterPager, CorePager>()
                .ForMember(dest => dest.PageIndex, opt => opt.MapFrom(src => src.GetRealPageIndex()))
                .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize.Value));

            //Language
            cfg.CreateMap<LanguageObj, WebAPI.Managers.Models.Language>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault));

            //KalturaLanguage
            cfg.CreateMap<LanguageObj, KalturaLanguage>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.DisplayName) ? src.DisplayName : src.Name))
                .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID));

            //KalturaCurrency
            cfg.CreateMap<Core.Pricing.Currency, KalturaCurrency>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.m_sCurrencyCD2))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.m_bIsDefault))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sCurrencyName))
                .ForMember(dest => dest.Sign, opt => opt.MapFrom(src => src.m_sCurrencySign))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nCurrencyID));

            //AssetType to Catalog.StatsType
            cfg.CreateMap<AssetType, StatsType>().ConstructUsing(ConvertAssetTypeToStatsType);

            #region Parental Rules

            // ParentalRule to KalturaParentalRule
            cfg.CreateMap<ParentalRule, WebAPI.Models.API.KalturaParentalRule>()
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.description, opt => opt.MapFrom(src => src.description))
                .ForMember(dest => dest.order, opt => opt.MapFrom(src => src.order))
                .ForMember(dest => dest.mediaTagTypeId, opt => opt.MapFrom(src => src.mediaTagTypeId))
                .ForMember(dest => dest.epgTagTypeId, opt => opt.MapFrom(src => src.epgTagTypeId))
                .ForMember(dest => dest.blockAnonymousAccess, opt => opt.MapFrom(src => src.blockAnonymousAccess))
                .ForMember(dest => dest.ruleType, opt => opt.ResolveUsing(src => ConvertParentalRuleType(src.ruleType.Value)))
                .ForMember(dest => dest.mediaTagValues, opt => opt.MapFrom(src => src.mediaTagValues.Select(x => new KalturaStringValue(null, false) { value = x }).ToList()))
                .ForMember(dest => dest.epgTagValues, opt => opt.MapFrom(src => src.epgTagValues.Select(x => new KalturaStringValue(null, false) { value = x }).ToList()))
                .ForMember(dest => dest.isDefault, opt => opt.MapFrom(src => src.isDefault))
                .ForMember(dest => dest.Origin, opt => opt.ResolveUsing(src => ConvertRuleLevel(src.level)))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.isActive))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate));

            // KalturaParentalRule to ParentalRule
            cfg.CreateMap<KalturaParentalRule, ParentalRule>()
                .ForMember(dest => dest.id, opt => opt.Ignore())
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.description, opt => opt.MapFrom(src => src.description))
                .ForMember(dest => dest.order, opt => opt.MapFrom(src => src.order))
                .ForMember(dest => dest.mediaTagTypeId, opt => opt.MapFrom(src => src.mediaTagTypeId))
                .ForMember(dest => dest.epgTagTypeId, opt => opt.MapFrom(src => src.epgTagTypeId))
                .ForMember(dest => dest.blockAnonymousAccess, opt => opt.MapFrom(src => src.blockAnonymousAccess))
                .ForMember(dest => dest.ruleType, opt => opt.ResolveUsing(src => ConvertParentalRuleType(src.ruleType)))
                .ForMember(dest => dest.mediaTagValues, opt => opt.MapFrom(src => src.mediaTagValues.Select(x => x.value).ToList()))
                .ForMember(dest => dest.epgTagValues, opt => opt.MapFrom(src => src.epgTagValues.Select(x => x.value).ToList()))
                .ForMember(dest => dest.isDefault, opt => opt.MapFrom(src => src.isDefault))
                .ForMember(dest => dest.level, opt => opt.Ignore())
                .ForMember(dest => dest.isActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate));

            // PinResponse
            cfg.CreateMap<PinResponse, WebAPI.Models.API.KalturaPinResponse>()
                .ForMember(dest => dest.Origin, opt => opt.ResolveUsing(src => ConvertRuleLevel(src.level)))
                .ForMember(dest => dest.PIN, opt => opt.MapFrom(src => src.pin))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaPinType.parental));

            // Pin
            cfg.CreateMap<PinResponse, WebAPI.Models.API.KalturaPin>()
                .ForMember(dest => dest.Origin, opt => opt.ResolveUsing(src => ConvertRuleLevel(src.level)))
                .ForMember(dest => dest.PIN, opt => opt.MapFrom(src => src.pin))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaPinType.parental));

            // Purchase Settings
            cfg.CreateMap<PurchaseSettingsResponse, WebAPI.Models.API.KalturaPurchaseSettings>()
                .ForMember(dest => dest.Origin, opt => opt.ResolveUsing(src => ConvertRuleLevel(src.level)))
                .ForMember(dest => dest.PIN, opt => opt.MapFrom(src => src.pin))
                .ForMember(dest => dest.Permission, opt => opt.ResolveUsing(src => ConvertPurchaseSetting(src.type)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaPinType.purchase));

            // Purchase Settings Response
            cfg.CreateMap<PurchaseSettingsResponse, WebAPI.Models.API.KalturaPurchaseSettingsResponse>()
                .ForMember(dest => dest.Origin, opt => opt.ResolveUsing(src => ConvertRuleLevel(src.level)))
                .ForMember(dest => dest.PIN, opt => opt.MapFrom(src => src.pin))
                .ForMember(dest => dest.PurchaseSettingsType, opt => opt.ResolveUsing(src => ConvertPurchaseSetting(src.type)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaPinType.purchase));

            cfg.CreateMap<GenericRule, WebAPI.Models.API.KalturaGenericRule>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.RuleType, opt => opt.ResolveUsing(src => ConvertRuleType(src.RuleType)));

            //KalturaUserAssetRule
            cfg.CreateMap<GenericRule, KalturaUserAssetRule>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.RuleType, opt => opt.ResolveUsing(src => ConvertRuleType(src.RuleType)));
            #endregion

            #region OSS Adapter

            cfg.CreateMap<WebAPI.Models.API.KalturaOSSAdapterProfile, OSSAdapter>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.SkipSettings, opt => opt.MapFrom(src => src.Settings == null))
               .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertOSSAdapterSettings(src.Settings)))
               .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier));

            cfg.CreateMap<OSSAdapter, WebAPI.Models.API.KalturaOSSAdapterProfile>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
              .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertOSSAdapterSettings(src.Settings)))
              .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier));

            cfg.CreateMap<OSSAdapterBase, WebAPI.Models.API.KalturaOSSAdapterBaseProfile>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            cfg.CreateMap<OSSAdapterResponse, WebAPI.Models.API.KalturaOSSAdapterProfile>()
             .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.OSSAdapter.AdapterUrl))
             .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.OSSAdapter.ExternalIdentifier))
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OSSAdapter.ID))
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.OSSAdapter.IsActive))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.OSSAdapter.Name))
             .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.OSSAdapter.SharedSecret))
             .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertOSSAdapterSettings(src.OSSAdapter.Settings)));

            #endregion

            #region Recommendation Engine

            cfg.CreateMap<WebAPI.Models.API.KalturaRecommendationProfile, RecommendationEngine>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.SkipSettings, opt => opt.MapFrom(src => src.Settings == null))
               .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertRecommendationEngineSettings(src.Settings)))
               .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier));

            cfg.CreateMap<RecommendationEngine, WebAPI.Models.API.KalturaRecommendationProfile>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
              .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertRecommendationEngineSettings(src.Settings)))
              .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier));

            cfg.CreateMap<RecommendationEngineResponse, WebAPI.Models.API.KalturaRecommendationProfile>()
             .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.RecommendationEngine.AdapterUrl))
             .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.RecommendationEngine.ExternalIdentifier))
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.RecommendationEngine.ID))
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.RecommendationEngine.IsActive))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.RecommendationEngine.Name))
             .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.RecommendationEngine.SharedSecret))
             .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertRecommendationEngineSettings(src.RecommendationEngine.Settings)));

            #endregion

            #region External Channel

            cfg.CreateMap<WebAPI.Models.API.KalturaExternalChannelProfile, ExternalChannel>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier))
               .ForMember(dest => dest.RecommendationEngineId, opt => opt.MapFrom(src => src.RecommendationEngineId))
               .ForMember(dest => dest.FilterExpression, opt => opt.MapFrom(src => src.FilterExpression))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.Enrichments, opt => opt.ResolveUsing(src => ConvertEnrichments(src.Enrichments)))
               .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId))
               .ForMember(dest => dest.MetaData, opt => opt.MapFrom(src => ConditionalAccessMappings.ConvertMetaData(src.MetaData)))
               .AfterMap((src, dest) => dest.MetaData = src.MetaData != null ? dest.MetaData : null);

            cfg.CreateMap<ExternalChannel, WebAPI.Models.API.KalturaExternalChannelProfile>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier))
               .ForMember(dest => dest.RecommendationEngineId, opt => opt.MapFrom(src => src.RecommendationEngineId))
               .ForMember(dest => dest.FilterExpression, opt => opt.MapFrom(src => src.FilterExpression))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.Enrichments, opt => opt.ResolveUsing(src => ConvertEnrichments(src.Enrichments)))
               .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId))
               .ForMember(dest => dest.MetaData, opt => opt.MapFrom(src => ConditionalAccessMappings.ConvertMetaData(src.MetaData)))
               .AfterMap((src, dest) => dest.MetaData = dest.MetaData != null && dest.MetaData.Any() ? dest.MetaData : null);

            cfg.CreateMap<ExternalChannelResponse, WebAPI.Models.API.KalturaExternalChannelProfile>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ExternalChannel.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ExternalChannel.Name))
               .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalChannel.ExternalIdentifier))
               .ForMember(dest => dest.RecommendationEngineId, opt => opt.MapFrom(src => src.ExternalChannel.RecommendationEngineId))
               .ForMember(dest => dest.FilterExpression, opt => opt.MapFrom(src => src.ExternalChannel.FilterExpression))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.ExternalChannel.IsActive))
               .ForMember(dest => dest.Enrichments, opt => opt.ResolveUsing(src => ConvertEnrichments(src.ExternalChannel.Enrichments)))
               .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.ExternalChannel.AssetUserRuleId))
               .ForMember(dest => dest.MetaData, opt => opt.MapFrom(src => ConditionalAccessMappings.ConvertMetaData(src.ExternalChannel.MetaData)))
               .AfterMap((src, dest) => dest.MetaData = dest.MetaData != null && dest.MetaData.Any() ? dest.MetaData : null);

            #endregion

            #region Export Tasks

            //Bulk export task
            cfg.CreateMap<BulkExportTask, KalturaExportTask>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.ExternalKey))
                .ForMember(dest => dest.DataType, opt => opt.ResolveUsing(src => ConvertExportDataType(src.DataType)))
                .ForMember(dest => dest.ExportType, opt => opt.ResolveUsing(src => ConvertExportType(src.ExportType)))
                .ForMember(dest => dest.Filter, opt => opt.MapFrom(src => src.Filter))
                .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.Frequency))
                .ForMember(dest => dest.NotificationUrl, opt => opt.MapFrom(src => src.NotificationUrl))
                .ForMember(dest => dest.VodTypes, opt => opt.MapFrom(src => src.VodTypes))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
            #endregion

            #region Roles and Permissions

            cfg.CreateMap<PermissionItem, KalturaPermissionItem>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.IsExcluded, opt => opt.MapFrom(src => src.IsExcluded));


            cfg.CreateMap<ApiActionPermissionItem, KalturaApiActionPermissionItem>()
               .IncludeBase<PermissionItem, KalturaPermissionItem>()
               .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service))
               .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action));

            cfg.CreateMap<ApiParameterPermissionItem, KalturaApiParameterPermissionItem>()
               .IncludeBase<PermissionItem, KalturaPermissionItem>()
               .ForMember(dest => dest.Object, opt => opt.MapFrom(src => src.Object))
               .ForMember(dest => dest.Parameter, opt => opt.MapFrom(src => src.Parameter))
               .ForMember(dest => dest.Action, opt => opt.ResolveUsing(src => ConvertApiParameterPermissionItemAction(src.Action)));

            cfg.CreateMap<ApiArgumentPermissionItem, KalturaApiArgumentPermissionItem>()
               .IncludeBase<PermissionItem, KalturaPermissionItem>()
               .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service))
               .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
               .ForMember(dest => dest.Parameter, opt => opt.MapFrom(src => src.Parameter));

            cfg.CreateMap<ApiPriviligesPermissionItem, KalturaApiPriviligesPermissionItem>()
               .IncludeBase<PermissionItem, KalturaPermissionItem>()
               .ForMember(dest => dest.Object, opt => opt.MapFrom(src => src.Object))
               .ForMember(dest => dest.Parameter, opt => opt.MapFrom(src => src.Parameter));

            cfg.CreateMap<Permission, KalturaPermission>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.FriendlyName, opt => opt.MapFrom(src => src.FriendlyName))
              //.ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertPermissionType(src.Type)))
              .ForMember(dest => dest.PermissionItems, opt => opt.ResolveUsing(src => ConvertPermissionItems(src.PermissionItems)))
              .ForMember(dest => dest.PermissionItemsIds, opt => opt.MapFrom(src => src.PermissionItemsIds != null ? string.Join(",", src.PermissionItemsIds) : null));

            cfg.CreateMap<KalturaPermission, Permission>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.FriendlyName, opt => opt.MapFrom(src => src.FriendlyName))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertPermissionType(src.Type)))
              .ForMember(dest => dest.PermissionItemsIds, opt => opt.MapFrom(src => src.GetPermissionItemsIds()));
;

            cfg.CreateMap<GroupPermission, KalturaGroupPermission>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.FriendlyName, opt => opt.MapFrom(src => src.FriendlyName))
              .ForMember(dest => dest.PermissionItems, opt => opt.ResolveUsing(src => ConvertPermissionItems(src.PermissionItems)))
              .ForMember(dest => dest.Group, opt => opt.MapFrom(src => src.UsersGroup))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => KalturaPermissionType.GROUP))
              .ForMember(dest => dest.DependsOnPermissionNames, opt => opt.MapFrom(src => src.DependsOnPermissionNames))
              ;

            cfg.CreateMap<Role, KalturaUserRole>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.Permissions, opt => opt.ResolveUsing(src => ConvertPermissions(src.Permissions)))
             .ForMember(dest => dest.PermissionNames, opt => opt.ResolveUsing(src => ConvertPermissionsNames(src.Permissions, false)))
             .ForMember(dest => dest.ExcludedPermissionNames, opt => opt.ResolveUsing(src => ConvertPermissionsNames(src.Permissions, true)))
             .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => src.GroupId == 0 ? KalturaUserRoleType.SYSTEM : KalturaUserRoleType.CUSTOM))
             .ForMember(dest => dest.Profile, opt => opt.ResolveUsing(src => ConvertRoleProfileType(src.Profile)));

            cfg.CreateMap<KalturaUserRole, Role>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Permissions, opt => opt.ResolveUsing(src => ConvertPermissionsNames(src.PermissionNames, src.ExcludedPermissionNames)))
            .ForMember(dest => dest.Profile, opt => opt.ResolveUsing(src => ConvertRoleProfileType(src.Profile)));

            cfg.CreateMap<KalturaPermissionItemFilter, PermissionItemFilter>();

            cfg.CreateMap<KalturaPermissionItemByIdInFilter, PermissionItemByIdInFilter>()
               .IncludeBase<KalturaPermissionItemFilter, PermissionItemFilter>()
               .ForMember(dest => dest.IdIn, opt => opt.ResolveUsing(src => !string.IsNullOrEmpty(src.IdIn) ? WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(src.IdIn, "KalturaPermissionItemByIdInFilter.IdIn", true) : null));

            cfg.CreateMap<KalturaPermissionItemByApiActionFilter, PermissionItemByApiActionFilter>()
               .IncludeBase<KalturaPermissionItemFilter, PermissionItemFilter>()
               .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.ActionEqual))
               .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.ServiceEqual));

            cfg.CreateMap<KalturaPermissionItemByArgumentFilter, PermissionItemByArgumentFilter>()
               .IncludeBase<KalturaPermissionItemByApiActionFilter, PermissionItemByApiActionFilter>()
               .ForMember(dest => dest.Parameter, opt => opt.MapFrom(src => src.ParameterEqual));

            cfg.CreateMap<KalturaPermissionItemByParameterFilter, PermissionItemByParameterFilter>()
               .IncludeBase<KalturaPermissionItemFilter, PermissionItemFilter>()
               .ForMember(dest => dest.Object, opt => opt.MapFrom(src => src.ObjectEqual))
               .ForMember(dest => dest.Parameter, opt => opt.MapFrom(src => src.ParameterEqual));

            #endregion

            //Api.RegistrySettings to KalturaRegistrySettings
            cfg.CreateMap<RegistrySettings, KalturaRegistrySettings>()
              .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.key))
              .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.value));

            #region Time Shifted Tv

            //TimeShiftedTvPartnerSettings to KalturaTimeShiftedTvPartnerSettings
            cfg.CreateMap<TimeShiftedTvPartnerSettings, WebAPI.Models.API.KalturaTimeShiftedTvPartnerSettings>()
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
                .ForMember(dest => dest.NonExistingChannelPlaybackEnabled, opt => opt.MapFrom(src => src.IsRecordingPlaybackNonExistingChannelEnabled))
                .ForMember(dest => dest.DefaultQuota, opt => opt.MapFrom(src => src.DefaultQuota))
                .ForMember(dest => dest.QuotaOveragePolicy, opt => opt.ResolveUsing(src => ConvertQuotaOveragePolicy(src.QuotaOveragePolicy)))
                .ForMember(dest => dest.ProtectionPolicy, opt => opt.ResolveUsing(src => ConvertProtectionPolicy(src.ProtectionPolicy)))
                .ForMember(dest => dest.RecoveryGracePeriod, opt => opt.MapFrom(src => src.RecoveryGracePeriod / (24 * 60 * 60)))// convert to days
                .ForMember(dest => dest.PrivateCopyEnabled, opt => opt.MapFrom(src => src.IsPrivateCopyEnabled))
                .ForMember(dest => dest.PersonalizedRecording, opt => opt.MapFrom(src => src.PersonalizedRecordingEnable))
                .ForMember(dest => dest.MaxRecordingConcurrency, opt => opt.MapFrom(src => src.MaxRecordingConcurrency))
                .ForMember(dest => dest.MaxConcurrencyMargin, opt => opt.MapFrom(src => src.MaxConcurrencyMargin))
                ;

            //KalturaTimeShiftedTvPartnerSettings to TimeShiftedTvPartnerSettings
            cfg.CreateMap<WebAPI.Models.API.KalturaTimeShiftedTvPartnerSettings, TimeShiftedTvPartnerSettings>()
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
                .ForMember(dest => dest.DefaultQuota, opt => opt.MapFrom(src => src.DefaultQuota))
                .ForMember(dest => dest.IsSeriesRecordingEnabled, opt => opt.MapFrom(src => src.SeriesRecordingEnabled))
                .ForMember(dest => dest.IsRecordingPlaybackNonEntitledChannelEnabled, opt => opt.MapFrom(src => src.NonEntitledChannelPlaybackEnabled))
                .ForMember(dest => dest.IsRecordingPlaybackNonExistingChannelEnabled, opt => opt.MapFrom(src => src.NonExistingChannelPlaybackEnabled))
                .ForMember(dest => dest.QuotaOveragePolicy, opt => opt.ResolveUsing(src => ConvertQuotaOveragePolicy(src.QuotaOveragePolicy)))
                .ForMember(dest => dest.ProtectionPolicy, opt => opt.ResolveUsing(src => ConvertProtectionPolicy(src.ProtectionPolicy)))
                .ForMember(dest => dest.RecoveryGracePeriod, opt => opt.MapFrom(src => src.RecoveryGracePeriod * 24 * 60 * 60))// convert days to seconds
                .ForMember(dest => dest.IsPrivateCopyEnabled, opt => opt.MapFrom(src => src.PrivateCopyEnabled))
                .ForMember(dest => dest.PersonalizedRecordingEnable, opt => opt.MapFrom(src => src.PersonalizedRecording))
                .ForMember(dest => dest.MaxRecordingConcurrency, opt => opt.MapFrom(src => src.MaxRecordingConcurrency))
                .ForMember(dest => dest.MaxConcurrencyMargin, opt => opt.MapFrom(src => src.MaxConcurrencyMargin))
                ;

            #endregion

            #region CDN Adapter

            cfg.CreateMap<KalturaCDNAdapterProfile, CDNAdapter>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
               .ForMember(dest => dest.BaseUrl, opt => opt.MapFrom(src => src.BaseUrl))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive.HasValue ? src.IsActive.Value : true))
               .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertCDNAdapterSettings(src.Settings)))
               .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
               .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret));

            cfg.CreateMap<CDNAdapter, KalturaCDNAdapterProfile>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
              .ForMember(dest => dest.BaseUrl, opt => opt.MapFrom(src => src.BaseUrl))
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
              .ForMember(dest => dest.Settings, opt => opt.ResolveUsing(src => ConvertCDNAdapterSettings(src.Settings)))
              .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
              .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret));

            #endregion

            #region CDN Settings

            //CDNPartnerSettings to KalturaCDNPartnerSettings
            cfg.CreateMap<CDNPartnerSettings, KalturaCDNPartnerSettings>()
                .ForMember(dest => dest.DefaultRecordingAdapterId, opt => opt.MapFrom(src => src.DefaultRecordingAdapter))
                .ForMember(dest => dest.DefaultAdapterId, opt => opt.MapFrom(src => src.DefaultAdapter));

            //KalturaCDNPartnerSettings to CDNPartnerSettings
            cfg.CreateMap<KalturaCDNPartnerSettings, CDNPartnerSettings>()
                .ForMember(dest => dest.DefaultRecordingAdapter, opt => opt.MapFrom(src => src.DefaultRecordingAdapterId))
                .ForMember(dest => dest.DefaultAdapter, opt => opt.MapFrom(src => src.DefaultAdapterId));

            #endregion

            #region Regions

            cfg.CreateMap<KeyValuePair<long, int>, KalturaRegionalChannel>()
                .ForMember(dest => dest.LinearChannelId, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.ChannelNumber, opt => opt.MapFrom(src => src.Value));

            cfg.CreateMap<Region, KalturaRegion>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
              .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.externalId))
              .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.isDefault))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
              .ForMember(dest => dest.RegionalChannels, opt => opt.MapFrom(src => src.linearChannels))
              .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.parentId));

            cfg.CreateMap<KalturaRegion, Region>()
                .ConstructUsing(src =>
                {
                    // Map null RegionalChannels to null linearChannels.
                    // By default Automapper mapping null collection to empty collection.
                    // Null and empty RegionalChannels have differen maining for the Region update functionality.
                    var result = new Region
                    {
                        linearChannels = src.RegionalChannels == null
                            ? null
                            : Mapper.Map<List<KeyValuePair<long, int>>>(src.RegionalChannels)
                    };
                    return result;
                })
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.externalId, opt => opt.MapFrom(src => src.ExternalId))
                .ForMember(dest => dest.isDefault, opt => opt.MapFrom(src => src.IsDefault))
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.parentId, opt => opt.MapFrom(src => src.ParentId));

            cfg.CreateMap<KalturaRegionalChannel, KeyValuePair<long, int>>()
                .ConvertUsing(x => new KeyValuePair<long, int>(x.LinearChannelId, x.ChannelNumber));

            cfg.CreateMap<KalturaRegionFilter, RegionFilter>()
                .ForMember(dest => dest.RegionIds, opt => opt.MapFrom(src => WebAPI.Utils.Utils.ParseCommaSeparatedValues<HashSet<int>, int>(src.IdIn, "KalturaRegionFilter.idIn", true, true)))
                .ForMember(dest => dest.ExternalIds, opt => opt.MapFrom(src => src.GetExternalIdIn()))
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentIdEqual))
                .ForMember(dest => dest.orderBy, opt => opt.MapFrom(src => ConvertRegionOrderBy(src.OrderBy)))
                .ForMember(dest => dest.LiveAssetId, opt => opt.MapFrom(src => src.LiveAssetIdEqual))
                .ForMember(dest => dest.ParentOnly, opt => opt.MapFrom(src => src.ParentOnly))
                .ForMember(dest => dest.ExclusiveLcn, opt => opt.MapFrom(src => src.ExclusiveLcn))
                ;

            cfg.CreateMap<KalturaRegionChannelNumber, RegionChannelNumber>()
                .ForMember(dest => dest.RegionId, opt => opt.MapFrom(src => src.RegionId))
                .ForMember(dest => dest.ChannelNumber, opt => opt.MapFrom(src => src.ChannelNumber));

            #endregion

            #region Device Family

            cfg.CreateMap<DeviceFamily, WebAPI.Models.Domains.KalturaDeviceFamily>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertToKalturaDeviceFamilyType(src.Id)));

            cfg.CreateMap<DeviceBrand, WebAPI.Models.Domains.KalturaDeviceBrand>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DeviceFamilyId, opt => opt.MapFrom(src => src.DeviceFamilyId))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertToKalturaDeviceBrandType(src.Id)));

            #endregion

            #region Kaltura Country

            cfg.CreateMap<CountryLocale, WebAPI.Models.Users.KalturaCountry>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
                .ForMember(dest => dest.CurrencySign, opt => opt.MapFrom(src => src.CurrencySign))
                .ForMember(dest => dest.LanguagesCode, opt => opt.MapFrom(src => src.LanguageCodes != null ? string.Join(",", src.LanguageCodes) : string.Empty))
                .ForMember(dest => dest.MainLanguageCode, opt => opt.MapFrom(src => src.MainLanguageCode))
                .ForMember(dest => dest.VatPercent, opt => opt.MapFrom(src => src.VatPercent))
                .ForMember(dest => dest.TimeZoneId, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.TimeZoneId) ? null : src.TimeZoneId));

            cfg.CreateMap<Country, WebAPI.Models.Users.KalturaCountry>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.TimeZoneId, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.TimeZoneId) ? null : src.TimeZoneId));

            #endregion

            #region Meta

            cfg.CreateMap<Meta, KalturaMeta>()
              .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.AssetType))
              .ForMember(dest => dest.FieldName, opt => opt.ResolveUsing(src => ConvertFieldName(src.FieldName)))
              .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Name)))
              .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertMetaType(src.Type)))
              .ForMember(dest => dest.Features, opt => opt.ResolveUsing(src => ConvertFeatures(src.Features)))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
              .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.PartnerId))
              .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => src.DynamicData != null ? src.DynamicData.ToDictionary(k => k.Key, v => v.Value) : null))
              ;

            cfg.CreateMap<KalturaMeta, Meta>()
             .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.AssetType))
             .ForMember(dest => dest.FieldName, opt => opt.ResolveUsing(src => ConvertMetaFieldName(src.FieldName)))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertMetaType(src.Type)))
             .ForMember(dest => dest.MultipleValue, opt => opt.MapFrom(src => src.MultipleValue))
             .ForMember(dest => dest.SkipFeatures, opt => opt.MapFrom(src => src.Features == null))
             .ForMember(dest => dest.Features, opt => opt.ResolveUsing(src => ConvertFeatures(src.Features)))
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
             .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.PartnerId))
             .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => WebAPI.Utils.Utils.ConvertSerializeableDictionary(src.DynamicData, true, false)))
              .AfterMap((src, dest) => dest.DynamicData = src.DynamicData != null ? dest.DynamicData : null)
             ;

            #endregion

            #region Search History

            cfg.CreateMap<SearchHistory, KalturaSearchHistory>()
              .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.action))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
              .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.deviceId))
              .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.language))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
              .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.createdAt))
              .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.service))
              .ForMember(dest => dest.Filter, opt => opt.MapFrom(src => src.filter.ToString()))
              ;

            #endregion

            #region DRM Adapter

            cfg.CreateMap<DrmAdapter, KalturaDrmProfile>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
              .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => src.Settings))
              .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.ExternalIdentifier))
              .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret));

            cfg.CreateMap<KalturaDrmProfile, DrmAdapter>()
              .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
              .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => src.Settings))
              .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.SystemName))
              .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret));

            cfg.CreateMap<DrmAdapterListResponse, KalturaDrmProfileListResponse>()
              .ForMember(dest => dest.Adapters, opt => opt.MapFrom(src => src.Adapters));

            #endregion

            #region AssetRule

            cfg.CreateMap<KalturaRule, Rule>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label));

            cfg.CreateMap<Rule, KalturaRule>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label));

            cfg.CreateMap<KalturaAssetRule, AssetRule>()
                .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => src.Actions))
                .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            cfg.CreateMap<AssetRule, KalturaAssetRule>()
                .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => src.Actions))
                .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            cfg.CreateMap<KalturaCondition, RuleCondition>()
                .Include(typeof(KalturaOrCondition), typeof(OrCondition))
                .Include(typeof(KalturaBusinessModuleCondition), typeof(BusinessModuleCondition))
                .Include(typeof(KalturaAssetConditionBase), typeof(AssetConditionBase))
                .Include(typeof(KalturaConcurrencyCondition), typeof(ConcurrencyCondition))
                .Include(typeof(KalturaDateCondition), typeof(DateCondition))
                .Include(typeof(KalturaIpRangeCondition), typeof(IpRangeCondition))
                .Include(typeof(KalturaIpV6RangeCondition), typeof(IpV6RangeCondition))
                .Include(typeof(KalturaCountryCondition), typeof(CountryCondition))
                .Include(typeof(KalturaHeaderCondition), typeof(HeaderCondition))
                .Include(typeof(KalturaUserSubscriptionCondition), typeof(UserSubscriptionCondition))
                .Include(typeof(KalturaAssetSubscriptionCondition), typeof(AssetSubscriptionCondition))
                .Include(typeof(KalturaUdidDynamicListCondition), typeof(UdidDynamicListCondition))
                .Include(typeof(KalturaUserRoleCondition), typeof(UserRoleCondition))
                .Include(typeof(KalturaUserSessionProfileCondition), typeof(UserSessionProfileCondition))
                .Include(typeof(KalturaSegmentsCondition), typeof(SegmentsCondition))
                .Include(typeof(KalturaDeviceBrandCondition), typeof(DeviceBrandCondition))
                .Include(typeof(KalturaDeviceFamilyCondition), typeof(DeviceFamilyCondition))
                .Include(typeof(KalturaDeviceManufacturerCondition), typeof(DeviceManufacturerCondition))
                .Include(typeof(KalturaDeviceModelCondition), typeof(DeviceModelCondition))
                .Include(typeof(KalturaDynamicKeysCondition), typeof(DynamicKeysCondition))
                .Include(typeof(KalturaDeviceDynamicDataCondition), typeof(DeviceDynamicDataCondition))
                .Include(typeof(KalturaChannelCondition), typeof(ChannelCondition))
                .Include(typeof(KalturaFileTypeCondition), typeof(FileTypeCondition));

            cfg.CreateMap<KalturaAssetConditionBase, AssetConditionBase>()
                .Include(typeof(KalturaAssetCondition), typeof(AssetCondition))
                .Include(typeof(KalturaAssetShopCondition), typeof(AssetShopCondition));

            cfg.CreateMap<RuleCondition, KalturaCondition>()
                .Include(typeof(OrCondition), typeof(KalturaOrCondition))
                .Include(typeof(BusinessModuleCondition), typeof(KalturaBusinessModuleCondition))
                .Include(typeof(AssetConditionBase), typeof(KalturaAssetConditionBase))
                .Include(typeof(ConcurrencyCondition), typeof(KalturaConcurrencyCondition))
                .Include(typeof(DateCondition), typeof(KalturaDateCondition))
                .Include(typeof(IpRangeCondition), typeof(KalturaIpRangeCondition))
                .Include(typeof(IpV6RangeCondition), typeof(KalturaIpV6RangeCondition))
                .Include(typeof(CountryCondition), typeof(KalturaCountryCondition))
                .Include(typeof(HeaderCondition), typeof(KalturaHeaderCondition))
                .Include(typeof(UserSubscriptionCondition), typeof(KalturaUserSubscriptionCondition))
                .Include(typeof(AssetSubscriptionCondition), typeof(KalturaAssetSubscriptionCondition))
                .Include(typeof(UdidDynamicListCondition), typeof(KalturaUdidDynamicListCondition))
                .Include(typeof(UserRoleCondition), typeof(KalturaUserRoleCondition))
                .Include(typeof(UserSessionProfileCondition), typeof(KalturaUserSessionProfileCondition))
                .Include(typeof(SegmentsCondition), typeof(KalturaSegmentsCondition))
                .Include(typeof(DeviceBrandCondition), typeof(KalturaDeviceBrandCondition))
                .Include(typeof(DeviceFamilyCondition), typeof(KalturaDeviceFamilyCondition))
                .Include(typeof(DeviceManufacturerCondition), typeof(KalturaDeviceManufacturerCondition))
                .Include(typeof(DeviceModelCondition), typeof(KalturaDeviceModelCondition))
                .Include(typeof(DynamicKeysCondition), typeof(KalturaDynamicKeysCondition))
                .Include(typeof(DeviceDynamicDataCondition), typeof(KalturaDeviceDynamicDataCondition))
                .Include(typeof(ChannelCondition), typeof(KalturaChannelCondition))
                .Include(typeof(FileTypeCondition), typeof(KalturaFileTypeCondition));

            cfg.CreateMap<AssetConditionBase, KalturaAssetConditionBase>()
                .Include(typeof(AssetCondition), typeof(KalturaAssetCondition))
                .Include(typeof(AssetShopCondition), typeof(KalturaAssetShopCondition));

            cfg.CreateMap<WebAPI.Models.Catalog.KalturaSlimAsset, SlimAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));

            cfg.CreateMap<KalturaRuleConditionType, RuleConditionType>()
               .ConvertUsing(kalturaRuleConditionType =>
               {
                   switch (kalturaRuleConditionType)
                   {
                       case KalturaRuleConditionType.ASSET: return RuleConditionType.Asset;
                       case KalturaRuleConditionType.COUNTRY: return RuleConditionType.Country;
                       case KalturaRuleConditionType.CONCURRENCY: return RuleConditionType.Concurrency;
                       case KalturaRuleConditionType.IP_RANGE: return RuleConditionType.IP_RANGE;
                       case KalturaRuleConditionType.IP_V6_RANGE: return RuleConditionType.IP_V6_RANGE;
                       case KalturaRuleConditionType.BUSINESS_MODULE: return RuleConditionType.BusinessModule;
                       case KalturaRuleConditionType.SEGMENTS: return RuleConditionType.Segments;
                       case KalturaRuleConditionType.DATE: return RuleConditionType.Date;
                       case KalturaRuleConditionType.OR: return RuleConditionType.Or;
                       case KalturaRuleConditionType.HEADER: return RuleConditionType.Header;
                       case KalturaRuleConditionType.USER_SUBSCRIPTION: return RuleConditionType.UserSubscription;
                       case KalturaRuleConditionType.ASSET_SUBSCRIPTION: return RuleConditionType.AssetSubscription;
                       case KalturaRuleConditionType.USER_ROLE: return RuleConditionType.UserRole;
                       case KalturaRuleConditionType.DEVICE_BRAND: return RuleConditionType.DeviceBrand;
                       case KalturaRuleConditionType.DEVICE_FAMILY: return RuleConditionType.DeviceFamily;
                       case KalturaRuleConditionType.DEVICE_MANUFACTURER: return RuleConditionType.DeviceManufacturer;
                       case KalturaRuleConditionType.DEVICE_MODEL: return RuleConditionType.DeviceModel;
                       case KalturaRuleConditionType.DEVICE_UDID_DYNAMIC_LIST: return RuleConditionType.DeviceUdidDynamicList;
                       case KalturaRuleConditionType.DYNAMIC_KEYS: return RuleConditionType.DynamicKeys;
                       case KalturaRuleConditionType.USER_SESSION_PROFILE: return RuleConditionType.UserSessionProfile;
                       case KalturaRuleConditionType.DEVICE_DYNAMIC_DATA: return RuleConditionType.DeviceDynamicData;
                       case KalturaRuleConditionType.ASSET_SHOP: return RuleConditionType.AssetShop;
                       case KalturaRuleConditionType.CHANNEL: return RuleConditionType.Channel;
                       case KalturaRuleConditionType.FILE_TYPE: return RuleConditionType.FileType;
                       default:
                           throw new ClientException((int)StatusCode.UnknownEnumValue,
                               string.Format("Unknown conditionType value : {0}", kalturaRuleConditionType.ToString()));
                   }
               });

            cfg.CreateMap<RuleConditionType, KalturaRuleConditionType>()
                .ConvertUsing(ruleConditionType =>
                {
                    switch (ruleConditionType)
                    {
                        case RuleConditionType.Asset: return KalturaRuleConditionType.ASSET;
                        case RuleConditionType.Country: return KalturaRuleConditionType.COUNTRY;
                        case RuleConditionType.Concurrency: return KalturaRuleConditionType.CONCURRENCY;
                        case RuleConditionType.IP_RANGE: return KalturaRuleConditionType.IP_RANGE;
                        case RuleConditionType.IP_V6_RANGE: return KalturaRuleConditionType.IP_V6_RANGE;
                        case RuleConditionType.BusinessModule: return KalturaRuleConditionType.BUSINESS_MODULE;
                        case RuleConditionType.Segments: return KalturaRuleConditionType.SEGMENTS;
                        case RuleConditionType.Date: return KalturaRuleConditionType.DATE;
                        case RuleConditionType.Or: return KalturaRuleConditionType.OR;
                        case RuleConditionType.Header: return KalturaRuleConditionType.HEADER;
                        case RuleConditionType.UserSubscription: return KalturaRuleConditionType.USER_SUBSCRIPTION;
                        case RuleConditionType.AssetSubscription: return KalturaRuleConditionType.ASSET_SUBSCRIPTION;
                        case RuleConditionType.UserRole: return KalturaRuleConditionType.USER_ROLE;
                        case RuleConditionType.DeviceBrand: return KalturaRuleConditionType.DEVICE_BRAND;
                        case RuleConditionType.DeviceFamily: return KalturaRuleConditionType.DEVICE_FAMILY;
                        case RuleConditionType.DeviceManufacturer: return KalturaRuleConditionType.DEVICE_MANUFACTURER;
                        case RuleConditionType.DeviceModel: return KalturaRuleConditionType.DEVICE_MODEL;
                        case RuleConditionType.DeviceUdidDynamicList: return KalturaRuleConditionType.DEVICE_UDID_DYNAMIC_LIST;
                        case RuleConditionType.DynamicKeys: return KalturaRuleConditionType.DYNAMIC_KEYS;
                        case RuleConditionType.UserSessionProfile: return KalturaRuleConditionType.USER_SESSION_PROFILE;
                        case RuleConditionType.DeviceDynamicData: return KalturaRuleConditionType.DEVICE_DYNAMIC_DATA;
                        case RuleConditionType.AssetShop: return KalturaRuleConditionType.ASSET_SHOP;
                        case RuleConditionType.Channel: return KalturaRuleConditionType.CHANNEL;
                        case RuleConditionType.FileType: return KalturaRuleConditionType.FILE_TYPE;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown conditionType value : {0}", ruleConditionType.ToString()));
                    }
                });

            cfg.CreateMap<KalturaOrCondition, OrCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
                .ForMember(dest => dest.Not, opt => opt.MapFrom(src => src.Not ?? false));

            cfg.CreateMap<OrCondition, KalturaOrCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
                .ForMember(dest => dest.Not, opt => opt.MapFrom(src => src.Not));

            cfg.CreateMap<KalturaBusinessModuleCondition, BusinessModuleCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.BusinessModuleId, opt => opt.MapFrom(src => src.BusinessModuleId.HasValue ? src.BusinessModuleId.Value : 0))
                .ForMember(dest => dest.BusinessModuleType, opt => opt.MapFrom(src => src.BusinessModuleType));

            cfg.CreateMap<BusinessModuleCondition, KalturaBusinessModuleCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.BusinessModuleId, opt => opt.MapFrom(src => src.BusinessModuleId))
                .ForMember(dest => dest.BusinessModuleType, opt => opt.MapFrom(src => src.BusinessModuleType));

            cfg.CreateMap<KalturaSegmentsCondition, SegmentsCondition>()
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
               .ForMember(dest => dest.SegmentIds, opt => opt.MapFrom(src => src.getSegmentsIds()));

            cfg.CreateMap<SegmentsCondition, KalturaSegmentsCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.SegmentsIds, opt => opt.MapFrom(src => src.SegmentIds != null ? string.Join(",", src.SegmentIds) : null));

            cfg.CreateMap<KalturaAssetCondition, AssetCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Ksql, opt => opt.MapFrom(src => src.Ksql));

            cfg.CreateMap<AssetCondition, KalturaAssetCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Ksql, opt => opt.MapFrom(src => src.Ksql));

            cfg.CreateMap<KalturaConcurrencyCondition, ConcurrencyCondition>()
                .IncludeBase<KalturaAssetCondition, AssetCondition>()
                .ForMember(dest => dest.Limit, opt => opt.MapFrom(src => src.Limit))
                .ForMember(dest => dest.RestrictionPolicy, opt => opt.MapFrom(src => src.ConcurrencyLimitationType));

            cfg.CreateMap<ConcurrencyCondition, KalturaConcurrencyCondition>()
                .IncludeBase<AssetCondition, KalturaAssetCondition>()
                .ForMember(dest => dest.Limit, opt => opt.MapFrom(src => src.Limit))
                .ForMember(dest => dest.ConcurrencyLimitationType, opt => opt.MapFrom(src => src.RestrictionPolicy));

            cfg.CreateMap<KalturaAssetShopCondition, AssetShopCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Values, opt => opt.ResolveUsing(src => ConvertStringValueArrayToList(src.Values)))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

            cfg.CreateMap<AssetShopCondition, KalturaAssetShopCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Values, opt => opt.ResolveUsing(src => ConvertListToStringValueArray(src.Values)))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

            cfg.CreateMap<KalturaConcurrencyLimitationType, ConcurrencyRestrictionPolicy>()
                .ConvertUsing(kalturaConcurrencyLimitationType =>
                {
                    switch (kalturaConcurrencyLimitationType)
                    {
                        case KalturaConcurrencyLimitationType.Single:
                            return ConcurrencyRestrictionPolicy.Single;
                            break;
                        case KalturaConcurrencyLimitationType.Group:
                            return ConcurrencyRestrictionPolicy.Group;
                            break;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown KalturaConcurrencyLimitationType value : {0}", kalturaConcurrencyLimitationType.ToString()));
                            break;
                    }
                });

            cfg.CreateMap<ConcurrencyRestrictionPolicy, KalturaConcurrencyLimitationType>()
                .ConvertUsing(concurrencyRestrictionPolicy =>
                {
                    switch (concurrencyRestrictionPolicy)
                    {
                        case ConcurrencyRestrictionPolicy.Single:
                            return KalturaConcurrencyLimitationType.Single;
                            break;
                        case ConcurrencyRestrictionPolicy.Group:
                            return KalturaConcurrencyLimitationType.Group;
                            break;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown ConcurrencyRestrictionPolicy value : {0}", concurrencyRestrictionPolicy.ToString()));
                            break;
                    }
                });

            cfg.CreateMap<KalturaDateCondition, DateCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Not, opt => opt.MapFrom(src => src.Not ?? false))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));

            cfg.CreateMap<DateCondition, KalturaDateCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Not, opt => opt.MapFrom(src => src.Not))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.HasValue ? src.StartDate.Value : 0))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.HasValue ? src.EndDate.Value : 0));

            cfg.CreateMap<KalturaIpV6RangeCondition, IpV6RangeCondition>()
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
               .ForMember(dest => dest.FromIp, opt => opt.MapFrom(src => src.FromIP))
               .ForMember(dest => dest.ToIp, opt => opt.MapFrom(src => src.ToIP));

            cfg.CreateMap<KalturaIpRangeCondition, IpRangeCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.FromIp, opt => opt.MapFrom(src => src.FromIP))
                .ForMember(dest => dest.ToIp, opt => opt.MapFrom(src => src.ToIP))
                .ForMember(dest => dest.IpFrom, opt => opt.ResolveUsing(src => GetConvertedIp(src.FromIP)))
                .ForMember(dest => dest.IpTo, opt => opt.ResolveUsing(src => GetConvertedIp(src.ToIP)));

            cfg.CreateMap<IpV6RangeCondition, KalturaIpV6RangeCondition>()
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
               .ForMember(dest => dest.FromIP, opt => opt.MapFrom(src => src.FromIp))
               .ForMember(dest => dest.ToIP, opt => opt.MapFrom(src => src.ToIp));

            cfg.CreateMap<IpRangeCondition, KalturaIpRangeCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.FromIP, opt => opt.MapFrom(src => src.FromIp))
                .ForMember(dest => dest.ToIP, opt => opt.MapFrom(src => src.ToIp));

            cfg.CreateMap<KalturaCountryCondition, CountryCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Not, opt => opt.MapFrom(src => src.Not ?? false))
                .ForMember(dest => dest.Countries, opt => opt.MapFrom(src => src.getCountries()));

            cfg.CreateMap<CountryCondition, KalturaCountryCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Not, opt => opt.MapFrom(src => src.Not))
                .ForMember(dest => dest.Countries, opt => opt.MapFrom(src => src.Countries != null ? string.Join(",", src.Countries) : null));

            cfg.CreateMap<KalturaHeaderCondition, HeaderCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Not, opt => opt.MapFrom(src => src.Not ?? false))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

            cfg.CreateMap<HeaderCondition, KalturaHeaderCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Not, opt => opt.MapFrom(src => src.Not))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

            // UserSubscriptionCondition
            cfg.CreateMap<KalturaUserSubscriptionCondition, UserSubscriptionCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.SubscriptionIds, opt => opt.MapFrom(src => WebAPI.Utils.Utils.ParseCommaSeparatedValues<HashSet<long>, long>(src.IdIn, "KalturaUserSubscriptionCondition.idIn", true, true)));

            cfg.CreateMap<UserSubscriptionCondition, KalturaUserSubscriptionCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IdIn, opt => opt.MapFrom(src => string.Join(",", src.SubscriptionIds)));

            // AssetSubscriptionCondition
            cfg.CreateMap<KalturaAssetSubscriptionCondition, AssetSubscriptionCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.SubscriptionIds, opt => opt.MapFrom(src => WebAPI.Utils.Utils.ParseCommaSeparatedValues<HashSet<long>, long>(src.IdIn, "KalturaUserSubscriptionCondition.idIn", true, true)));

            cfg.CreateMap<AssetSubscriptionCondition, KalturaAssetSubscriptionCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IdIn, opt => opt.MapFrom(src => string.Join(",", src.SubscriptionIds)));

            //Campaign conditions
            cfg.CreateMap<KalturaDeviceBrandCondition, DeviceBrandCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IdIn, opt => opt.ResolveUsing(src => src.GetDeviceBrandIds()));;

            cfg.CreateMap<DeviceBrandCondition, KalturaDeviceBrandCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IdIn, opt => opt.MapFrom(src => string.Join(",", src.IdIn)));

            cfg.CreateMap<KalturaDeviceFamilyCondition, DeviceFamilyCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IdIn, opt => opt.ResolveUsing(src => src.GetDeviceFamilyIds()));

            cfg.CreateMap<DeviceFamilyCondition, KalturaDeviceFamilyCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IdIn, opt => opt.MapFrom(src => string.Join(",", src.IdIn)));

            cfg.CreateMap<KalturaDeviceManufacturerCondition, DeviceManufacturerCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IdIn, opt => opt.ResolveUsing(src => src.GetDeviceManufacturerIds()));

            cfg.CreateMap<DeviceManufacturerCondition, KalturaDeviceManufacturerCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IdIn, opt => opt.MapFrom(src => string.Join(",", src.IdIn)));

            cfg.CreateMap<KalturaDeviceModelCondition, DeviceModelCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.RegexEqual, opt => opt.MapFrom(src => src.RegexEqual));

            cfg.CreateMap<DeviceModelCondition, KalturaDeviceModelCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.RegexEqual, opt => opt.MapFrom(src => src.RegexEqual));

            cfg.CreateMap<KalturaUdidDynamicListCondition, UdidDynamicListCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            cfg.CreateMap<UdidDynamicListCondition, KalturaUdidDynamicListCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            cfg.CreateMap<KalturaUserRoleCondition, UserRoleCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.RoleIds, opt => opt.MapFrom(src => WebAPI.Utils.Utils.ParseCommaSeparatedValues<HashSet<long>, long>(src.IdIn, "KalturaUserRoleCondition.idIn", true, true)));

            cfg.CreateMap<UserRoleCondition, KalturaUserRoleCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IdIn, opt => opt.MapFrom(src => string.Join(",", src.RoleIds)));

            cfg.CreateMap<KalturaDynamicKeysCondition, DynamicKeysCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.Values, opt => opt.ResolveUsing(src => src.GetValues()));

            cfg.CreateMap<DynamicKeysCondition, KalturaDynamicKeysCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.Values, opt => opt.ResolveUsing(src => ToCSV(src.Values)));

            cfg.CreateMap<KalturaDeviceDynamicDataCondition, DeviceDynamicDataCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));
            cfg.CreateMap<DeviceDynamicDataCondition, KalturaDeviceDynamicDataCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

            cfg.CreateMap<KalturaUserSessionProfileCondition, UserSessionProfileCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            cfg.CreateMap<UserSessionProfileCondition, KalturaUserSessionProfileCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            cfg.CreateMap<KalturaChannelCondition, ChannelCondition>()
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
               .ForMember(dest => dest.ChannelIds, opt => opt.ResolveUsing(src => src.GetChannelIds())); ;

            cfg.CreateMap<ChannelCondition, KalturaChannelCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IdIn, opt => opt.MapFrom(src => string.Join(",", src.ChannelIds)));

            cfg.CreateMap<KalturaFileTypeCondition, FileTypeCondition>()
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
              .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
              .ForMember(dest => dest.FileTypeIds, opt => opt.ResolveUsing(src => src.GetFileTypeIds())); ;

            cfg.CreateMap<FileTypeCondition, KalturaFileTypeCondition>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IdIn, opt => opt.MapFrom(src => string.Join(",", src.FileTypeIds)));

            cfg.CreateMap<KalturaRuleActionType, RuleActionType>()
           .ConvertUsing(kalturaRuleActionType =>
           {

               switch (kalturaRuleActionType)
               {
                   case KalturaRuleActionType.BLOCK: return RuleActionType.Block;
                   case KalturaRuleActionType.START_DATE_OFFSET: return RuleActionType.StartDateOffset;
                   case KalturaRuleActionType.END_DATE_OFFSET: return RuleActionType.EndDateOffset;
                   case KalturaRuleActionType.USER_BLOCK: return RuleActionType.UserBlock;
                   case KalturaRuleActionType.ALLOW_PLAYBACK: return RuleActionType.AllowPlayback;
                   case KalturaRuleActionType.BLOCK_PLAYBACK: return RuleActionType.BlockPlayback;
                   case KalturaRuleActionType.APPLY_DISCOUNT_MODULE:return RuleActionType.ApplyDiscountModuleRule;
                   case KalturaRuleActionType.APPLY_PLAYBACK_ADAPTER: return RuleActionType.ApplyPlaybackAdapter;
                   case KalturaRuleActionType.FILTER: return RuleActionType.UserFilter;
                   case KalturaRuleActionType.ASSET_LIFE_CYCLE_TRANSITION: return RuleActionType.AssetLifeCycleTransition;
                   case KalturaRuleActionType.APPLY_FREE_PLAYBACK: return RuleActionType.ApplyFreePlayback;
                   case KalturaRuleActionType.FilterAssetByKsql: return RuleActionType.FilterAssetByKsql;
                   case KalturaRuleActionType.FilterFileByQualityInDiscovery: return RuleActionType.FilterFileByQualityInDiscovery;
                   case KalturaRuleActionType.FilterFileByQualityInPlayback: return RuleActionType.FilterFileByQualityInPlayback;
                   case KalturaRuleActionType.FilterFileByFileTypeIdForAssetTypeInDiscovery: return RuleActionType.FilterFileByAssetTypeInDiscovery;
                   case KalturaRuleActionType.FilterFileByFileTypeIdForAssetTypeInPlayback: return RuleActionType.FilterFileByAssetTypeInPlayback;
                   case KalturaRuleActionType.FilterFileByFileTypeIdInDiscovery: return RuleActionType.FilterFileByFileTypeIdInDiscovery;
                   case KalturaRuleActionType.FilterFileByFileTypeIdInPlayback: return RuleActionType.FilterFileByFileTypeIdInPlayback;
                   case KalturaRuleActionType.FilterFileByAudioCodecInDiscovery: return RuleActionType.FilterFileByAudioCodecInDiscovery;
                   case KalturaRuleActionType.FilterFileByAudioCodecInPlayback: return RuleActionType.FilterFileByAudioCodecInPlayback;
                   case KalturaRuleActionType.FilterFileByVideoCodecInDiscovery: return RuleActionType.FilterFileByVideoCodecInDiscovery;
                   case KalturaRuleActionType.FilterFileByVideoCodecInPlayback: return RuleActionType.FilterFileByVideoCodecInPlayback;
                   case KalturaRuleActionType.FilterFileByStreamerTypeInDiscovery: return RuleActionType.FilterFileByStreamerTypeInDiscovery;
                   case KalturaRuleActionType.FilterFileByStreamerTypeInPlayback: return RuleActionType.FilterFileByStreamerTypeInPlayback;
                   case KalturaRuleActionType.FilterFileByLabelInDiscovery: return RuleActionType.FilterFileByLabelInDiscovery;
                   case KalturaRuleActionType.FilterFileByLabelInPlayback: return RuleActionType.FilterFileByLabelInPlayback;
                   case KalturaRuleActionType.FilterFileByDynamicDataInDiscovery: return RuleActionType.FilterFileByDynamicDataInDiscovery;
                   case KalturaRuleActionType.FilterFileByDynamicDataInPlayback: return RuleActionType.FilterFileByDynamicDataInPlayback;
                   default: throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown RuleAction value : {0}", kalturaRuleActionType.ToString()));
               }
           });

            cfg.CreateMap<KalturaAssetRuleOrderBy, AssetRuleOrderBy>()
                .ConvertUsing(orderBy =>
                {
                    switch (orderBy)
                    {
                        case KalturaAssetRuleOrderBy.NONE: return AssetRuleOrderBy.None;
                        case KalturaAssetRuleOrderBy.NAME_ASC: return AssetRuleOrderBy.NameAsc;
                        case KalturaAssetRuleOrderBy.NAME_DESC: return AssetRuleOrderBy.NameDesc;
                        default: throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown AssetRuleOrderBy value : {0}", orderBy.ToString()));
                    }
                });

            cfg.CreateMap<RuleActionType, KalturaRuleActionType>()
                .ConvertUsing(ruleActionType =>
                {
                    switch (ruleActionType)
                    {
                        case RuleActionType.Block: return KalturaRuleActionType.BLOCK;
                        case RuleActionType.StartDateOffset: return KalturaRuleActionType.START_DATE_OFFSET;
                        case RuleActionType.EndDateOffset: return KalturaRuleActionType.END_DATE_OFFSET;
                        case RuleActionType.UserBlock: return KalturaRuleActionType.USER_BLOCK;
                        case RuleActionType.AllowPlayback: return KalturaRuleActionType.ALLOW_PLAYBACK;
                        case RuleActionType.BlockPlayback: return KalturaRuleActionType.BLOCK_PLAYBACK;
                        case RuleActionType.ApplyDiscountModuleRule: return KalturaRuleActionType.APPLY_DISCOUNT_MODULE;
                        case RuleActionType.ApplyPlaybackAdapter: return KalturaRuleActionType.APPLY_PLAYBACK_ADAPTER;
                        case RuleActionType.UserFilter: return KalturaRuleActionType.FILTER;
                        case RuleActionType.AssetLifeCycleTransition: return KalturaRuleActionType.ASSET_LIFE_CYCLE_TRANSITION;
                        case RuleActionType.ApplyFreePlayback: return KalturaRuleActionType.APPLY_FREE_PLAYBACK;
                        case RuleActionType.FilterAssetByKsql: return KalturaRuleActionType.FilterAssetByKsql;
                        case RuleActionType.FilterFileByQualityInDiscovery: return KalturaRuleActionType.FilterFileByQualityInDiscovery;
                        case RuleActionType.FilterFileByQualityInPlayback: return KalturaRuleActionType.FilterFileByQualityInPlayback;
                        case RuleActionType.FilterFileByAssetTypeInDiscovery: return KalturaRuleActionType.FilterFileByFileTypeIdForAssetTypeInDiscovery;
                        case RuleActionType.FilterFileByAssetTypeInPlayback: return KalturaRuleActionType.FilterFileByFileTypeIdForAssetTypeInPlayback;
                        case RuleActionType.FilterFileByFileTypeIdInDiscovery: return KalturaRuleActionType.FilterFileByFileTypeIdInDiscovery;
                        case RuleActionType.FilterFileByFileTypeIdInPlayback: return KalturaRuleActionType.FilterFileByFileTypeIdInPlayback;
                        case RuleActionType.FilterFileByAudioCodecInDiscovery: return KalturaRuleActionType.FilterFileByAudioCodecInDiscovery;
                        case RuleActionType.FilterFileByAudioCodecInPlayback: return KalturaRuleActionType.FilterFileByAudioCodecInPlayback;
                        case RuleActionType.FilterFileByVideoCodecInDiscovery: return KalturaRuleActionType.FilterFileByVideoCodecInDiscovery;
                        case RuleActionType.FilterFileByVideoCodecInPlayback: return KalturaRuleActionType.FilterFileByVideoCodecInPlayback;
                        case RuleActionType.FilterFileByStreamerTypeInDiscovery: return KalturaRuleActionType.FilterFileByStreamerTypeInDiscovery;
                        case RuleActionType.FilterFileByStreamerTypeInPlayback: return KalturaRuleActionType.FilterFileByStreamerTypeInPlayback;
                        case RuleActionType.FilterFileByLabelInDiscovery: return KalturaRuleActionType.FilterFileByLabelInDiscovery;
                        case RuleActionType.FilterFileByLabelInPlayback: return KalturaRuleActionType.FilterFileByLabelInPlayback;
                        case RuleActionType.FilterFileByDynamicDataInDiscovery: return KalturaRuleActionType.FilterFileByDynamicDataInDiscovery;
                        case RuleActionType.FilterFileByDynamicDataInPlayback: return KalturaRuleActionType.FilterFileByDynamicDataInPlayback;
                        default: throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown ruleActionType value : {0}", ruleActionType.ToString()));
                    }
                });

            cfg.CreateMap<KalturaBasePreActionCondition, BasePreActionCondition>();

            cfg.CreateMap<KalturaShopPreActionCondition, ShopPreActionCondition>()
                .IncludeBase<KalturaBasePreActionCondition, BasePreActionCondition>()
                .ForMember(dest => dest.ShopAssetUserRuleId, opt => opt.MapFrom(src => src.ShopAssetUserRuleId));

            cfg.CreateMap<KalturaNoShopPreActionCondition, NoShopPreActionCondition>()
                .IncludeBase<KalturaBasePreActionCondition, BasePreActionCondition>();

            cfg.CreateMap<BasePreActionCondition, KalturaBasePreActionCondition>();

            cfg.CreateMap<ShopPreActionCondition, KalturaShopPreActionCondition>()
                .IncludeBase<BasePreActionCondition, KalturaBasePreActionCondition>()
                .ForMember(dest => dest.ShopAssetUserRuleId, opt => opt.MapFrom(src => src.ShopAssetUserRuleId));

            cfg.CreateMap<NoShopPreActionCondition, KalturaNoShopPreActionCondition>()
                .IncludeBase<BasePreActionCondition, KalturaBasePreActionCondition>();

            cfg.CreateMap<KalturaRuleAction, RuleAction>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            cfg.CreateMap<RuleAction, KalturaRuleAction>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            cfg.CreateMap<KalturaRuleAction, AssetRuleAction>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            cfg.CreateMap<AssetRuleAction, KalturaRuleAction>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            cfg.CreateMap<KalturaRuleAction, AssetUserRuleAction>()
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            cfg.CreateMap<KalturaAssetRuleAction, AssetRuleAction>()
                .IncludeBase<KalturaRuleAction, RuleAction>();

            cfg.CreateMap<KalturaAssetRuleAction, AssetRuleFilterAction>()
                .IncludeBase<KalturaRuleAction, RuleAction>();

            cfg.CreateMap<AssetRuleAction, KalturaAssetRuleAction>()
                .IncludeBase<RuleAction, KalturaRuleAction>();

            cfg.CreateMap<KalturaFilterAction, AssetRuleFilterAction>()
                .IncludeBase<KalturaAssetRuleAction, AssetRuleAction>()
                .ForMember(dest => dest.PreActionCondition, opt => opt.MapFrom(src => src.PreActionCondition));

            cfg.CreateMap<AssetRuleFilterAction, KalturaFilterAction>()
                .IncludeBase<AssetRuleAction, KalturaAssetRuleAction>()
                .ForMember(dest => dest.PreActionCondition, opt => opt.MapFrom(src => src.PreActionCondition));

            cfg.CreateMap<AssetRuleFilterAction, KalturaAssetRuleAction>()
                .IncludeBase<AssetRuleAction, KalturaAssetRuleAction>();

            cfg.CreateMap<AssetUserRuleAction, KalturaRuleAction>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            cfg.CreateMap<KalturaApplyPlaybackAdapterAction, ApplyPlaybackAdapterRuleAction>()
               .IncludeBase<KalturaAssetRuleAction, AssetRuleAction>()
               .ForMember(dest => dest.AdapterId, opt => opt.MapFrom(src => src.AdapterId));

            cfg.CreateMap<ApplyPlaybackAdapterRuleAction, KalturaApplyPlaybackAdapterAction>()
                .IncludeBase<AssetRuleAction, KalturaAssetRuleAction>()
                .ForMember(dest => dest.AdapterId, opt => opt.MapFrom(src => src.AdapterId));

            cfg.CreateMap<KalturaBusinessModuleRuleAction, BusinessModuleRuleAction>()
               .IncludeBase<KalturaRuleAction, RuleAction>();

            cfg.CreateMap<BusinessModuleRuleAction, KalturaBusinessModuleRuleAction>()
                .IncludeBase<RuleAction, KalturaRuleAction>();

            cfg.CreateMap<KalturaApplyDiscountModuleAction, ApplyDiscountModuleRuleAction>()
                .IncludeBase<KalturaBusinessModuleRuleAction, BusinessModuleRuleAction>()
                .ForMember(dest => dest.DiscountModuleId, opt => opt.MapFrom(src => src.DiscountModuleId));

            cfg.CreateMap<ApplyDiscountModuleRuleAction, KalturaApplyDiscountModuleAction>()
                .IncludeBase<BusinessModuleRuleAction, KalturaBusinessModuleRuleAction>()
                .ForMember(dest => dest.DiscountModuleId, opt => opt.MapFrom(src => src.DiscountModuleId));

            cfg.CreateMap<KalturaApplyFreePlaybackAction, ApplyFreePlaybackAction>()
                .IncludeBase<KalturaBusinessModuleRuleAction, BusinessModuleRuleAction>();

            cfg.CreateMap<ApplyFreePlaybackAction, KalturaApplyFreePlaybackAction>()
                .IncludeBase<BusinessModuleRuleAction, KalturaBusinessModuleRuleAction>();

            cfg.CreateMap<KalturaAssetUserRuleAction, AssetUserRuleAction>()
                .IncludeBase<KalturaRuleAction, RuleAction>();

            cfg.CreateMap<AssetUserRuleAction, KalturaAssetUserRuleAction>()
                .IncludeBase<RuleAction, KalturaRuleAction>();

            cfg.CreateMap<KalturaAssetUserRuleBlockAction, AssetUserRuleBlockAction>()
               .IncludeBase<KalturaAssetUserRuleAction, AssetUserRuleAction>();

            cfg.CreateMap<AssetUserRuleBlockAction, KalturaAssetUserRuleBlockAction>()
                .IncludeBase<AssetUserRuleAction, KalturaAssetUserRuleAction>();

            cfg.CreateMap<KalturaAssetUserRuleFilterAction, AssetUserRuleFilterAction>()
               .IncludeBase<KalturaAssetUserRuleAction, AssetUserRuleAction>()
               .ForMember(dest => dest.ApplyOnChannel, opt => opt.MapFrom(src => src.ApplyOnChannel));

            cfg.CreateMap<AssetUserRuleFilterAction, KalturaAssetUserRuleFilterAction>()
                .IncludeBase<AssetUserRuleAction, KalturaAssetUserRuleAction>()
                .ForMember(dest => dest.ApplyOnChannel, opt => opt.MapFrom(src => src.ApplyOnChannel));

            cfg.CreateMap<KalturaAllowPlaybackAction, AllowPlaybackAction>()
               .IncludeBase<KalturaAssetRuleAction, AssetRuleAction>();

            cfg.CreateMap<AllowPlaybackAction, KalturaAllowPlaybackAction>()
                .IncludeBase<AssetRuleAction, KalturaAssetRuleAction>();

            cfg.CreateMap<KalturaBlockPlaybackAction, BlockPlaybackAction>()
                .IncludeBase<KalturaAssetRuleAction, AssetRuleAction>();

            cfg.CreateMap<BlockPlaybackAction, KalturaBlockPlaybackAction>()
                .IncludeBase<AssetRuleAction, KalturaAssetRuleAction>();

            cfg.CreateMap<KalturaAccessControlBlockAction, AssetBlockAction>()
                .IncludeBase<KalturaAssetRuleAction, AssetRuleAction>();

            cfg.CreateMap<AssetBlockAction, KalturaAccessControlBlockAction>()
                .IncludeBase<AssetRuleAction, KalturaAssetRuleAction>();

            cfg.CreateMap<KalturaTimeOffsetRuleAction, TimeOffsetRuleAction>()
                .IncludeBase<KalturaAssetRuleAction, AssetRuleAction>()
                .ForMember(dest => dest.Offset, opt => opt.MapFrom(src => src.Offset))
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone));

            cfg.CreateMap<TimeOffsetRuleAction, KalturaTimeOffsetRuleAction>()
                .IncludeBase<AssetRuleAction, KalturaAssetRuleAction>()
                .ForMember(dest => dest.Offset, opt => opt.MapFrom(src => src.Offset))
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone));

            cfg.CreateMap<KalturaStartDateOffsetRuleAction, StartDateOffsetRuleAction>()
                .IncludeBase<KalturaTimeOffsetRuleAction, TimeOffsetRuleAction>();

            cfg.CreateMap<StartDateOffsetRuleAction, KalturaStartDateOffsetRuleAction>()
                .IncludeBase<TimeOffsetRuleAction, KalturaTimeOffsetRuleAction>();

            cfg.CreateMap<KalturaEndDateOffsetRuleAction, EndDateOffsetRuleAction>()
                .IncludeBase<KalturaTimeOffsetRuleAction, TimeOffsetRuleAction>();

            cfg.CreateMap<EndDateOffsetRuleAction, KalturaEndDateOffsetRuleAction>()
                .IncludeBase<TimeOffsetRuleAction, KalturaTimeOffsetRuleAction>();

            cfg.CreateMap<KalturaAssetLifeCycleTransitionAction, AssetLifeCycleTransitionAction>()
                .IncludeBase<KalturaAssetRuleAction, AssetRuleAction>()
                .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => src.AssetLifeCycleRuleActionType))
                .ForMember(dest => dest.TransitionType, opt => opt.MapFrom(src => src.AssetLifeCycleRuleTransitionType));

            cfg.CreateMap<AssetLifeCycleTransitionAction, KalturaAssetLifeCycleTransitionAction>()
                .IncludeBase<AssetRuleAction, KalturaAssetRuleAction>()
                .ForMember(dest => dest.AssetLifeCycleRuleActionType, opt => opt.MapFrom(src => src.ActionType))
                .ForMember(dest => dest.AssetLifeCycleRuleTransitionType, opt => opt.MapFrom(src => src.TransitionType));

            cfg.CreateMap<KalturaAssetLifeCycleRuleActionType, AssetLifeCycleRuleAction>()
                .ConvertUsing(assetLifeCycleRuleActionType =>
                {
                    switch (assetLifeCycleRuleActionType)
                    {
                        case KalturaAssetLifeCycleRuleActionType.ADD:
                            return AssetLifeCycleRuleAction.Add;
                        case KalturaAssetLifeCycleRuleActionType.REMOVE:
                            return AssetLifeCycleRuleAction.Remove;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown KalturaAssetLifeCycleRuleActionType value : {0}", assetLifeCycleRuleActionType.ToString()));
                    }
                });

            cfg.CreateMap<AssetLifeCycleRuleAction, KalturaAssetLifeCycleRuleActionType>()
                .ConvertUsing(assetLifeCycleRuleActionType =>
                {
                    switch (assetLifeCycleRuleActionType)
                    {
                        case AssetLifeCycleRuleAction.Add:
                            return KalturaAssetLifeCycleRuleActionType.ADD;
                        case AssetLifeCycleRuleAction.Remove:
                            return KalturaAssetLifeCycleRuleActionType.REMOVE;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown AssetLifeCycleRuleAction value : {0}", assetLifeCycleRuleActionType.ToString()));
                    }
                });

            cfg.CreateMap<KalturaAssetLifeCycleTagTransitionAction, AssetLifeCycleTagTransitionAction>()
                .IncludeBase<KalturaAssetLifeCycleTransitionAction, AssetLifeCycleTransitionAction>()
                .ForMember(dest => dest.TagIds, opt => opt.MapFrom(src => WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(src.TagIds, "tagsIds", true, false)));

            cfg.CreateMap<AssetLifeCycleTagTransitionAction, KalturaAssetLifeCycleTagTransitionAction>()
                .IncludeBase<AssetLifeCycleTransitionAction, KalturaAssetLifeCycleTransitionAction>()
                .ForMember(dest => dest.TagIds, opt => opt.MapFrom(src => string.Join(",", src.TagIds)));

            cfg.CreateMap<KalturaAssetLifeCycleBuisnessModuleTransitionAction, AssetLifeCycleBuisnessModuleTransitionAction>()
               .IncludeBase<KalturaAssetLifeCycleTransitionAction, AssetLifeCycleTransitionAction>()
               .ForPath(dest => dest.Transitions.FileTypeIds, opt => opt.MapFrom(src => WebAPI.Utils.Utils.ParseCommaSeparatedValues<HashSet<int>, int>(src.FileTypeIds, "fileTypeIds", true, false)))
               .ForPath(dest => dest.Transitions.PpvIds, opt => opt.MapFrom(src => WebAPI.Utils.Utils.ParseCommaSeparatedValues<HashSet<int>, int>(src.PpvIds, "ppvIds", true, false)));

            cfg.CreateMap<AssetLifeCycleBuisnessModuleTransitionAction, KalturaAssetLifeCycleBuisnessModuleTransitionAction>()
                .IncludeBase<AssetLifeCycleTransitionAction, KalturaAssetLifeCycleTransitionAction>()
                .ForMember(dest => dest.FileTypeIds, opt => opt.MapFrom(src => string.Join(",", src.Transitions.FileTypeIds)))
                .ForMember(dest => dest.PpvIds, opt => opt.MapFrom(src => string.Join(",", src.Transitions.PpvIds)));

            cfg.CreateMap<KalturaFilterFileByVideoCodecAction, FilterFileByVideoCodec>()
                .IncludeBase<KalturaFilterAction, AssetRuleFilterAction>()
                .ForMember(dest => dest.VideoCodecs, opt => opt.ResolveUsing(src => src.GetVideoCodecs()));

            cfg.CreateMap<FilterFileByVideoCodec, KalturaFilterFileByVideoCodecAction>()
                .IncludeBase<AssetRuleFilterAction, KalturaFilterAction>()
                .ForMember(dest => dest.VideoCodecIn, opt => opt.MapFrom(src => string.Join(",", src.VideoCodecs)));

            cfg.CreateMap<KalturaFilterFileByVideoCodecInDiscoveryAction, FilterFileByVideoCodecInDiscovery>()
                .IncludeBase<KalturaFilterFileByVideoCodecAction, FilterFileByVideoCodec>();

            cfg.CreateMap<KalturaFilterFileByVideoCodecInPlayback, FilterFileByVideoCodecInPlayback>()
                .IncludeBase<KalturaFilterFileByVideoCodecAction, FilterFileByVideoCodec>();

          cfg.CreateMap<FilterFileByVideoCodecInDiscovery, KalturaFilterFileByVideoCodecInDiscoveryAction>()
                .IncludeBase<FilterFileByVideoCodec, KalturaFilterFileByVideoCodecAction>();

            cfg.CreateMap<FilterFileByVideoCodecInPlayback, KalturaFilterFileByVideoCodecInPlayback>()
                .IncludeBase<FilterFileByVideoCodec, KalturaFilterFileByVideoCodecAction>();

            cfg.CreateMap<KalturaFilterFileByStreamerTypeAction, FilterFileByStreamerType>()
                .IncludeBase<KalturaFilterAction, AssetRuleFilterAction>()
                .ForMember(dest => dest.StreamerTypes, opt => opt.ResolveUsing(src => src.GetStreamerTypes()));

            cfg.CreateMap<FilterFileByStreamerType, KalturaFilterFileByStreamerTypeAction>()
                .IncludeBase<AssetRuleFilterAction, KalturaFilterAction>()
                .ForMember(dest => dest.StreamerTypeIn, opt => opt.ResolveUsing(src => GetStreamerTypes(src.StreamerTypes)));

            cfg.CreateMap<KalturaFilterFileByStreamerTypeInDiscovery, FilterFileByStreamerTypeInDiscovery>()
                .IncludeBase<KalturaFilterFileByStreamerTypeAction, FilterFileByStreamerType>();

            cfg.CreateMap<KalturaFilterFileByStreamerTypeInPlayback, FilterFileByStreamerTypeInPlayback>()
                .IncludeBase<KalturaFilterFileByStreamerTypeAction, FilterFileByStreamerType>();

            cfg.CreateMap<FilterFileByStreamerTypeInDiscovery, KalturaFilterFileByStreamerTypeInDiscovery>()
                .IncludeBase<FilterFileByStreamerType, KalturaFilterFileByStreamerTypeAction>();

            cfg.CreateMap<FilterFileByStreamerTypeInPlayback, KalturaFilterFileByStreamerTypeInPlayback>()
                .IncludeBase<FilterFileByStreamerType, KalturaFilterFileByStreamerTypeAction>();

            cfg.CreateMap<KalturaFilterFileByQualityAction, FilterFileByQuality>()
                .IncludeBase<KalturaFilterAction, AssetRuleFilterAction>()
                .ForMember(dest => dest.Qualities, opt => opt.ResolveUsing(src => src.GetQualities()));

            cfg.CreateMap<FilterFileByQuality, KalturaFilterFileByQualityAction>()
                .IncludeBase<AssetRuleFilterAction, KalturaFilterAction>()
                .ForMember(dest => dest.QualityIn, opt => opt.ResolveUsing(src => GetQualities(src.Qualities)));

            cfg.CreateMap<KalturaFilterFileByQualityInDiscoveryAction, FilterFileByQualityInDiscovery>()
                .IncludeBase<KalturaFilterFileByQualityAction, FilterFileByQuality>();

            cfg.CreateMap<KalturaFilterFileByQualityInPlaybackAction, FilterFileByQualityInPlayback>()
                .IncludeBase<KalturaFilterFileByQualityAction, FilterFileByQuality>();

            cfg.CreateMap<FilterFileByQualityInDiscovery, KalturaFilterFileByQualityInDiscoveryAction>()
                .IncludeBase<FilterFileByQuality, KalturaFilterFileByQualityAction>();

            cfg.CreateMap<FilterFileByQualityInPlayback, KalturaFilterFileByQualityInPlaybackAction>()
                .IncludeBase<FilterFileByQuality, KalturaFilterFileByQualityAction>();

            cfg.CreateMap<KalturaFilterFileByLabelAction, FilterFileByLabel>()
                .IncludeBase<KalturaFilterAction, AssetRuleFilterAction>()
                .ForMember(dest => dest.Labels, opt => opt.ResolveUsing(src => src.GetLabels()));

            cfg.CreateMap<FilterFileByLabel, KalturaFilterFileByLabelAction>()
                .IncludeBase<AssetRuleFilterAction, KalturaFilterAction>()
                .ForMember(dest => dest.LabelIn, opt => opt.MapFrom(src => string.Join(",", src.Labels)));

            cfg.CreateMap<KalturaFilterFileByLabelInDiscoveryAction, FilterFileByLabelInDiscovery>()
                .IncludeBase<KalturaFilterFileByLabelAction, FilterFileByLabel>();

            cfg.CreateMap<KalturaFilterFileByLabelInPlaybackAction, FilterFileByLabelInPlayback>()
                .IncludeBase<KalturaFilterFileByLabelAction, FilterFileByLabel>();

            cfg.CreateMap<FilterFileByLabelInDiscovery, KalturaFilterFileByLabelInDiscoveryAction>()
                .IncludeBase<FilterFileByLabel, KalturaFilterFileByLabelAction>();

            cfg.CreateMap<FilterFileByLabelInPlayback, KalturaFilterFileByLabelInPlaybackAction>()
                .IncludeBase<FilterFileByLabel, KalturaFilterFileByLabelAction>();

            cfg.CreateMap<KalturaFilterFileByFileTypeIdForAssetTypeAction, FilterFileByFileTypeForAssetType>()
                .IncludeBase<KalturaFilterFileByFileTypeIdAction, FilterFileByFileType>()
                .ForMember(dest => dest.AssetTypes, opt => opt.ResolveUsing(src => src.GetAssetTypes()));

            cfg.CreateMap<FilterFileByFileTypeForAssetType, KalturaFilterFileByFileTypeIdForAssetTypeAction>()
                .IncludeBase<FilterFileByFileType, KalturaFilterFileByFileTypeIdAction>()
                .ForMember(dest => dest.AssetTypeIn, opt => opt.ResolveUsing(src => GetAssetTypes(src.AssetTypes)));

            cfg.CreateMap<KalturaFilterFileByFileTypeIdForAssetTypeInDiscoveryAction, FilterFileByFileTypeForAssetTypeInDiscovery>()
                .IncludeBase<KalturaFilterFileByFileTypeIdForAssetTypeAction, FilterFileByFileTypeForAssetType>();

            cfg.CreateMap<KalturaFilterFileByFileTypeIdForAssetTypeInPlaybackAction, FilterFileByFileTypeForAssetTypenPlayback>()
                .IncludeBase<KalturaFilterFileByFileTypeIdForAssetTypeAction, FilterFileByFileTypeForAssetType>();

            cfg.CreateMap<FilterFileByFileTypeForAssetTypeInDiscovery, KalturaFilterFileByFileTypeIdForAssetTypeInDiscoveryAction>()
                .IncludeBase<FilterFileByFileTypeForAssetType, KalturaFilterFileByFileTypeIdForAssetTypeAction>();

            cfg.CreateMap<FilterFileByFileTypeForAssetTypenPlayback, KalturaFilterFileByFileTypeIdForAssetTypeInPlaybackAction>()
                .IncludeBase<FilterFileByFileTypeForAssetType, KalturaFilterFileByFileTypeIdForAssetTypeAction>();

            cfg.CreateMap<KalturaFilterFileByFileTypeIdAction, FilterFileByFileType>()
                .IncludeBase<KalturaFilterAction, AssetRuleFilterAction>()
                .ForMember(dest => dest.FileTypeIds, opt => opt.ResolveUsing(src => src.GetFileTypesIds()));

            cfg.CreateMap<FilterFileByFileType, KalturaFilterFileByFileTypeIdAction>()
                .IncludeBase<AssetRuleFilterAction, KalturaFilterAction>()
                .ForMember(dest => dest.FileTypeIdIn, opt => opt.MapFrom(src => string.Join(",", src.FileTypeIds)));

            cfg.CreateMap<KalturaFilterFileByFileTypeIdInDiscoveryAction, FilterFileByFileTypeInDiscovery>()
                .IncludeBase<KalturaFilterFileByFileTypeIdAction, FilterFileByFileType>();

            cfg.CreateMap<KalturaFilterFileByFileTypeIdInPlaybackAction, FilterFileByFileTypeInPlayback>()
                .IncludeBase<KalturaFilterFileByFileTypeIdAction, FilterFileByFileType>();

            cfg.CreateMap<FilterFileByFileTypeInDiscovery, KalturaFilterFileByFileTypeIdInDiscoveryAction>()
                .IncludeBase<FilterFileByFileType, KalturaFilterFileByFileTypeIdAction>();

            cfg.CreateMap<FilterFileByFileTypeInPlayback, KalturaFilterFileByFileTypeIdInPlaybackAction>()
                .IncludeBase<FilterFileByFileType, KalturaFilterFileByFileTypeIdAction>();

            cfg.CreateMap<KalturaFilterFileByAudioCodecAction, FilterFileByAudioCodec>()
                .IncludeBase<KalturaFilterAction, AssetRuleFilterAction>()
                .ForMember(dest => dest.AudioCodecs, opt => opt.ResolveUsing(src => src.GetAudioCodecs()));

            cfg.CreateMap<FilterFileByAudioCodec, KalturaFilterFileByAudioCodecAction>()
                .IncludeBase<AssetRuleFilterAction, KalturaFilterAction>()
                .ForMember(dest => dest.AudioCodecIn, opt => opt.MapFrom(src => string.Join(",", src.AudioCodecs)));

            cfg.CreateMap<KalturaFilterFileByAudioCodecInDiscoveryAction, FilterFileByAudioCodecInDiscovery>()
                .IncludeBase<KalturaFilterFileByAudioCodecAction, FilterFileByAudioCodec>();

            cfg.CreateMap<KalturaFilterFileByAudioCodecInPlaybackAction, FilterFileByAudioCodecInPlayback>()
                .IncludeBase<KalturaFilterFileByAudioCodecAction, FilterFileByAudioCodec>();

            cfg.CreateMap<FilterFileByAudioCodecInDiscovery, KalturaFilterFileByAudioCodecInDiscoveryAction>()
                .IncludeBase<FilterFileByAudioCodec, KalturaFilterFileByAudioCodecAction>();

            cfg.CreateMap<FilterFileByAudioCodecInPlayback, KalturaFilterFileByAudioCodecInPlaybackAction>()
                .IncludeBase<FilterFileByAudioCodec, KalturaFilterFileByAudioCodecAction>();

            cfg.CreateMap<KalturaFilterAssetByKsqlAction, FilterAssetByKsql>()
                .IncludeBase<KalturaFilterAction, AssetRuleFilterAction>()
                .ForMember(dest => dest.Ksql, opt => opt.MapFrom(src => src.Ksql));

            cfg.CreateMap<FilterAssetByKsql, KalturaFilterAssetByKsqlAction>()
                .IncludeBase<AssetRuleFilterAction, KalturaFilterAction>()
                .ForMember(dest => dest.Ksql, opt => opt.MapFrom(src => src.Ksql));

            cfg.CreateMap<KalturaFilterFileByDynamicDataAction, FilterFileByDynamicData>()
                .IncludeBase<KalturaFilterAction, AssetRuleFilterAction>()
                .ForMember(dest => dest.Key, opt => opt.ResolveUsing(src => src.Key))
                .ForMember(dest => dest.Values, opt => opt.ResolveUsing(src => src.GetValues()));

            cfg.CreateMap<KalturaFilterFileByDynamicDataInDiscoveryAction, FilterFileByDynamicDataInDiscovery>()
                .IncludeBase<KalturaFilterFileByDynamicDataAction, FilterFileByDynamicData>();

            cfg.CreateMap<KalturaFilterFileByDynamicDataInPlaybackAction, FilterFileByDynamicDataInPlayback>()
                .IncludeBase<KalturaFilterFileByDynamicDataAction, FilterFileByDynamicData>();

            cfg.CreateMap<FilterFileByDynamicData, KalturaFilterFileByDynamicDataAction>()
                .IncludeBase<AssetRuleFilterAction, KalturaFilterAction>()
                .ForMember(dest => dest.Key, opt => opt.ResolveUsing(src => src.Key))
                .ForMember(dest => dest.Values, opt => opt.MapFrom(src => string.Join(",", src.Values)));

            cfg.CreateMap<FilterFileByDynamicDataInDiscovery, KalturaFilterFileByDynamicDataInDiscoveryAction>()
                .IncludeBase<FilterFileByDynamicData, KalturaFilterFileByDynamicDataAction>();

            cfg.CreateMap<FilterFileByDynamicDataInPlayback, KalturaFilterFileByDynamicDataInPlaybackAction>()
                .IncludeBase<FilterFileByDynamicData, KalturaFilterFileByDynamicDataAction>();

            cfg.CreateMap<KalturaRuleType?, RuleType?>()
                .ConvertUsing(kalturaRuleType =>
                {
                    if (!kalturaRuleType.HasValue)
                    {
                        return null;
                    }

                    switch (kalturaRuleType)
                    {
                        case KalturaRuleType.parental:
                            return RuleType.Parental;
                            break;
                        case KalturaRuleType.geo:
                            return RuleType.Geo;
                            break;
                        case KalturaRuleType.user_type:
                            return RuleType.UserType;
                            break;
                        case KalturaRuleType.device:
                            return RuleType.Device;
                            break;
                        case KalturaRuleType.assetUser:
                            return RuleType.AssetUser;
                            break;
                        case KalturaRuleType.network:
                            return RuleType.Network;
                            break;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue,
                                string.Format("Unknown KalturaRuleType value : {0}", kalturaRuleType.ToString()));
                            break;
                    }
                });

            cfg.CreateMap<KalturaTvmRuleType?, TvmRuleType?>()
               .ConvertUsing(kalturaTvmRuleType =>
               {
                   if (!kalturaTvmRuleType.HasValue)
                   {
                       return null;
                   }

                   switch (kalturaTvmRuleType)
                   {
                       case KalturaTvmRuleType.Geo:
                           return TvmRuleType.Geo;
                           break;
                       case KalturaTvmRuleType.Device:
                           return TvmRuleType.Device;
                           break;
                       default:
                           throw new ClientException((int)StatusCode.UnknownEnumValue,
                               string.Format("Unknown KalturaTvmRuleType value : {0}", kalturaTvmRuleType.ToString()));
                           break;
                   }
               });

            cfg.CreateMap<TvmRuleType?, KalturaTvmRuleType?>()
               .ConvertUsing(tvmRuleType =>
               {
                   if (!tvmRuleType.HasValue)
                   {
                       return null;
                   }

                   switch (tvmRuleType)
                   {
                       case TvmRuleType.Geo:
                           return KalturaTvmRuleType.Geo;
                           break;
                       case TvmRuleType.Device:
                           return KalturaTvmRuleType.Device;
                           break;
                       default:
                           throw new ClientException((int)StatusCode.UnknownEnumValue,
                               string.Format("Unknown TvmRuleType value : {0}", tvmRuleType.ToString()));
                           break;
                   }
               });

            cfg.CreateMap<KalturaTvmRule, TvmRule>()
                .IncludeBase<KalturaRule, Rule>()
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.RuleType, opt => opt.MapFrom(src => src.RuleType));

            cfg.CreateMap<TvmRule, KalturaTvmRule>()
                .IncludeBase<Rule, KalturaRule>()
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.RuleType, opt => opt.MapFrom(src => src.RuleType));

            cfg.CreateMap<KalturaTvmGeoRule, TvmGeoRule>()
                .IncludeBase<KalturaTvmRule, TvmRule>()
                .ForMember(dest => dest.OnlyOrBut, opt => opt.MapFrom(src => src.OnlyOrBut))
                .ForMember(dest => dest.CountryIds, opt => opt.MapFrom(src => src.GetCountryIds()))
                .ForMember(dest => dest.ProxyRuleId, opt => opt.MapFrom(src => src.ProxyRuleId))
                .ForMember(dest => dest.ProxyRuleName, opt => opt.MapFrom(src => src.ProxyRuleName))
                .ForMember(dest => dest.ProxyLevelId, opt => opt.MapFrom(src => src.ProxyLevelId))
                .ForMember(dest => dest.ProxyLevelName, opt => opt.MapFrom(src => src.ProxyLevelName));

            cfg.CreateMap<TvmGeoRule, KalturaTvmGeoRule>()
                .IncludeBase<TvmRule, KalturaTvmRule>()
                .ForMember(dest => dest.OnlyOrBut, opt => opt.MapFrom(src => src.OnlyOrBut))
                .ForMember(dest => dest.CountryIds, opt => opt.MapFrom(src => src.CountryIds != null ? string.Join(",", src.CountryIds) : null))
                .ForMember(dest => dest.ProxyRuleId, opt => opt.MapFrom(src => src.ProxyRuleId))
                .ForMember(dest => dest.ProxyRuleName, opt => opt.MapFrom(src => src.ProxyRuleName))
                .ForMember(dest => dest.ProxyLevelId, opt => opt.MapFrom(src => src.ProxyLevelId))
                .ForMember(dest => dest.ProxyLevelName, opt => opt.MapFrom(src => src.ProxyLevelName));

            cfg.CreateMap<KalturaTvmDeviceRule, TvmDeviceRule>()
                .IncludeBase<KalturaTvmRule, TvmRule>()
                .ForMember(dest => dest.DeviceBrandIds, opt => opt.MapFrom(src => src.GetDeviceBrandIds()));

            cfg.CreateMap<TvmDeviceRule, KalturaTvmDeviceRule>()
                .IncludeBase<TvmRule, KalturaTvmRule>()
                .ForMember(dest => dest.DeviceBrandIds, opt => opt.MapFrom(src => src.DeviceBrandIds != null ? string.Join(",", src.DeviceBrandIds) : null));

            //Campaign
            cfg.CreateMap<KalturaCampaign, Campaign>()
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
               .ForMember(dest => dest.Promotion, opt => opt.MapFrom(src => src.Promotion))
               .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
               .ForMember(dest => dest.State, opt => opt.MapFrom(src => ConvertObjectState(src.State)))
               .ForMember(dest => dest.CollectionIds, opt => opt.ResolveUsing(src => src.GetCollectionIds()))
               .ForMember(dest => dest.NullableProperties, opt => opt.MapFrom(src => src.NullableProperties))
               .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId))
               .AfterMap((src, dest) => dest.CollectionIds = dest.CollectionIds != null && dest.CollectionIds.Any() ? dest.CollectionIds : null);

            cfg.CreateMap<Campaign, KalturaCampaign>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
               .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
               .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
               .ForMember(dest => dest.State, opt => opt.ResolveUsing(src => ConvertObjectState(src.State)))
               .ForMember(dest => dest.Promotion, opt => opt.MapFrom(src => src.Promotion))
               .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
               .ForMember(dest => dest.CollectionIdIn, opt => opt.MapFrom(src => src.CollectionIds != null ? string.Join(",", src.CollectionIds) : null))
               .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId));

            cfg.CreateMap<KalturaApiService, ApiService>()
                .ConvertUsing(apiService =>
                {
                    switch (apiService)
                    {
                        case KalturaApiService.HOUSEHOLD_DEVICE:
                            return ApiService.DomainDevice;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown KalturaApiService value: {apiService}.");
                    }
                });

            cfg.CreateMap<ApiService, KalturaApiService>()
                .ConvertUsing(apiService =>
                {
                    switch (apiService)
                    {
                        case ApiService.DomainDevice:
                            return KalturaApiService.HOUSEHOLD_DEVICE;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown ApiService value: {apiService}.");
                    }
                });

            cfg.CreateMap<KalturaApiAction, ApiAction>()
                .ConvertUsing(apiAction =>
                {
                    switch (apiAction)
                    {
                        case KalturaApiAction.ADD:
                            return ApiAction.Insert;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown KalturaApiAction value: {apiAction}.");
                    }
                });

            cfg.CreateMap<ApiAction, KalturaApiAction>()
                .ConvertUsing(apiAction =>
                {
                    switch (apiAction)
                    {
                        case ApiAction.Insert:
                            return KalturaApiAction.ADD;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown ApiAction value: {apiAction}.");
                    }
                });

            cfg.CreateMap<KalturaBatchCampaign, BatchCampaign>()
                .IncludeBase<KalturaCampaign, Campaign>()
                .ForMember(dest => dest.PopulationConditions, opt => opt.MapFrom(src => src.PopulationConditions))
                .AfterMap((src, dest) => dest.PopulationConditions = dest.PopulationConditions != null && dest.PopulationConditions.Any() ? dest.PopulationConditions : null);
            ;

            cfg.CreateMap<BatchCampaign, KalturaBatchCampaign>()
                .IncludeBase<Campaign, KalturaCampaign>()
                .ForMember(dest => dest.PopulationConditions, opt => opt.MapFrom(src => src.PopulationConditions))
            ;

            cfg.CreateMap<KalturaTriggerCampaign, TriggerCampaign>()
                .IncludeBase<KalturaCampaign, Campaign>()
                .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service))
                .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
                .ForMember(dest => dest.TriggerConditions, opt => opt.MapFrom(src => src.TriggerConditions))
                .AfterMap((src, dest) => dest.TriggerConditions = dest.TriggerConditions != null && dest.TriggerConditions.Any() ? dest.TriggerConditions : null);
            ;

            cfg.CreateMap<TriggerCampaign, KalturaTriggerCampaign>()
                .IncludeBase<Campaign, KalturaCampaign>()
                .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service))
                .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
                .ForMember(dest => dest.TriggerConditions, opt => opt.MapFrom(src => src.TriggerConditions))
                ;

            cfg.CreateMap<KalturaCampaignFilter, CampaignFilter>()
                .ForMember(dest => dest.OrderBy, opt => opt.MapFrom(src => ConvertCampaignOrder(src.OrderBy)))
                ;

            cfg.CreateMap<CampaignFilter, KalturaCampaignFilter>()
                .ForMember(dest => dest.OrderBy, opt => opt.MapFrom(src => ConvertCampaignOrder(src.OrderBy)))
                ;

            cfg.CreateMap<KalturaCampaignIdInFilter, CampaignIdInFilter>()
                .IncludeBase<KalturaCampaignFilter, CampaignFilter>()
                .ForMember(dest => dest.IdIn, opt => opt.ResolveUsing(src => !string.IsNullOrEmpty(src.IdIn) ? WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(src.IdIn, "filter.idIn") : null))
                ;

            cfg.CreateMap<KalturaCampaignSearchFilter, CampaignSearchFilter>()
                .IncludeBase<KalturaCampaignFilter, CampaignFilter>()
                .ForMember(dest => dest.StartDateGreaterThanOrEqual, opt => opt.MapFrom(src => src.StartDateGreaterThanOrEqual))
                .ForMember(dest => dest.EndDateLessThanOrEqual, opt => opt.MapFrom(src => src.EndDateLessThanOrEqual))
                .ForMember(dest => dest.StateEqual, opt => opt.MapFrom(src => src.StateEqual))
                .ForMember(dest => dest.HasPromotion, opt => opt.MapFrom(src => src.HasPromotion))
                .ForMember(dest => dest.NameEqual, opt => opt.MapFrom(src => src.NameEqual))
                .ForMember(dest => dest.NameContains, opt => opt.MapFrom(src => src.NameContains))
                .ForMember(dest => dest.StateIn, opt => opt.ResolveUsing(src => src.GetStates()))
                .ForMember(dest => dest.AssetUserRuleIds, opt => opt.ResolveUsing(src => src.GetAssetUserRuleIds()));

            cfg.CreateMap<KalturaTriggerCampaignSearchFilter, TriggerCampaignFilter>()
              .IncludeBase<KalturaCampaignSearchFilter, CampaignSearchFilter>()
              ;

            cfg.CreateMap<KalturaBatchCampaignSearchFilter, BatchCampaignFilter>()
              .IncludeBase<KalturaCampaignSearchFilter, CampaignSearchFilter>()
              ;

            cfg.CreateMap<KalturaCampaignSegmentFilter, CampaignSegmentFilter>()
                .IncludeBase<KalturaCampaignSearchFilter, CampaignSearchFilter>()
                .ForMember(dest => dest.SegmentIdEqual, opt => opt.MapFrom(src => src.SegmentIdEqual));

            cfg.CreateMap<KalturaObjectState, CampaignState>()
               .ConvertUsing(state =>
               {
                   switch (state)
                   {
                       case KalturaObjectState.INACTIVE:
                           return CampaignState.INACTIVE;
                       case KalturaObjectState.ACTIVE:
                           return CampaignState.ACTIVE;
                       case KalturaObjectState.ARCHIVE:
                           return CampaignState.ARCHIVE;
                       default:
                           throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown KalturaObjectState value: {state}.");
                   }
               });

            #endregion

            cfg.CreateMap<KalturaBasePromotion, BasePromotion>()
               .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions));

            cfg.CreateMap<BasePromotion, KalturaBasePromotion>()
               .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions));

            cfg.CreateMap<KalturaPromotion, Promotion>()
                .IncludeBase<KalturaBasePromotion, BasePromotion>()
                .ForMember(dest => dest.DiscountModuleId, opt => opt.MapFrom(src => src.DiscountModuleId))
                .ForMember(dest => dest.NumberOfRecurring, opt => opt.MapFrom(src => src.NumberOfRecurring))
                .ForMember(dest => dest.MaxDiscountUsages, opt => opt.MapFrom(src => src.MaxDiscountUsages));

            cfg.CreateMap<Promotion, KalturaPromotion>()
                .IncludeBase<BasePromotion, KalturaBasePromotion>()
                .ForMember(dest => dest.DiscountModuleId, opt => opt.MapFrom(src => src.DiscountModuleId))
                .ForMember(dest => dest.NumberOfRecurring, opt => opt.MapFrom(src => src.NumberOfRecurring))
                .ForMember(dest => dest.MaxDiscountUsages, opt => opt.MapFrom(src => src.MaxDiscountUsages));

            cfg.CreateMap<KalturaCouponPromotion, CouponPromotion>()
               .IncludeBase<KalturaBasePromotion, BasePromotion>()
               .ForMember(dest => dest.CouponGroupId, opt => opt.MapFrom(src => src.CouponGroupId));

            cfg.CreateMap<CouponPromotion, KalturaCouponPromotion>()
                .IncludeBase<BasePromotion, KalturaBasePromotion>()
                .ForMember(dest => dest.CouponGroupId, opt => opt.MapFrom(src => src.CouponGroupId));

            #region AssetUserRule

            cfg.CreateMap<AssetUserRule, KalturaAssetUserRule>()
              .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => src.Actions))
              .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
              .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            cfg.CreateMap<KalturaAssetUserRule, AssetUserRule>()
              .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => src.Actions))
              .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
              .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            #endregion

            #region Media Concurrency Rule

            cfg.CreateMap<MediaConcurrencyRule, KalturaMediaConcurrencyRule>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.RuleID))
            .ForMember(dest => dest.Limitation, opt => opt.MapFrom(src => src.Limitation))
            .ForMember(dest => dest.ConcurrencyLimitationType, opt => opt.MapFrom(src => src.RestrictionPolicy))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            #endregion

            #region Personal List

            cfg.CreateMap<PersonalListItem, Models.Api.KalturaPersonalList>()
              .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.Timestamp))
              .ForMember(dest => dest.Ksql, opt => opt.MapFrom(src => src.Ksql))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.PartnerListType, opt => opt.MapFrom(src => src.PartnerListType))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            cfg.CreateMap<Models.Api.KalturaPersonalList, PersonalListItem>()
              .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.CreateDate))
              .ForMember(dest => dest.Ksql, opt => opt.MapFrom(src => src.Ksql))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.PartnerListType, opt => opt.MapFrom(src => src.PartnerListType))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
            #endregion

            #region Business Module Rule

            cfg.CreateMap<BusinessModuleRule, KalturaBusinessModuleRule>()
              .ForMember(dest => dest.Actions, opt => opt.ResolveUsing(src => src.Actions))
              .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
              .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
              .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
              .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            cfg.CreateMap<KalturaBusinessModuleRule, BusinessModuleRule>()
              .ForMember(dest => dest.Actions, opt => opt.ResolveUsing(src => src.Actions))
              .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
              .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
              .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
              .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            cfg.CreateMap<KalturaBusinessModuleRuleFilter, BusinessModuleRuleConditionScope>()
                .ForMember(dest => dest.BusinessModuleId, opt => opt.MapFrom(src => src.BusinessModuleIdApplied.HasValue ? src.BusinessModuleIdApplied.Value : 0))
                .ForMember(dest => dest.BusinessModuleType, opt => opt.MapFrom(src => src.BusinessModuleTypeApplied))
                .ForMember(dest => dest.SegmentIds, opt => opt.ResolveUsing(src => !string.IsNullOrEmpty(src.SegmentIdsApplied) ? WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(src.SegmentIdsApplied, "filter.segmentIdsApplied") : null))
                .ForMember(dest => dest.FilterBySegments, opt => opt.ResolveUsing(src => !string.IsNullOrEmpty(src.SegmentIdsApplied) ? true : false));

            #endregion

            #region Playback Profile

            cfg.CreateMap<PlaybackProfile, KalturaPlaybackProfile>()
             .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.ExternalIdentifier))
             .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => src.Settings))
             .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret));

            cfg.CreateMap<KalturaPlaybackProfile, PlaybackProfile>()
             .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.SystemName))
             .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => src.Settings))
             .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret));
            #endregion

            #region Ingest Profile

            cfg.CreateMap<IngestProfile, KalturaIngestProfile>()
                .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => src.Settings != null ? src.Settings.ToDictionary(k => k.Key, v => v.Value) : null))
                .ForMember(dest => dest.OverlapChannels, opt => opt.MapFrom(src => string.Join(",", src.OverlapChannels)))
                .ForMember(dest => dest.DefaultAutoFillPolicy, opt => opt.MapFrom(src => (int)src.DefaultAutoFillPolicy))
                .ForMember(dest => dest.DefaultOverlapPolicy, opt => opt.MapFrom(src => (int)src.DefaultOverlapPolicy));

            cfg.CreateMap<KalturaIngestProfile, IngestProfile>()
                .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => src.Settings != null ? src.Settings.Select(s => new IngestProfileAdapterParam
                {
                    IngestProfileId = src.Id ?? -1,
                    Key = s.Key,
                    Value = s.Value == null ? "" : s.Value.value,
                }) : null))
                .ForMember(dest => dest.OverlapChannels, opt => opt.MapFrom(src => src.GetOverlapChannels()))
                .ForMember(dest => dest.DefaultAutoFillPolicy, opt => opt.MapFrom(src => (int)src.DefaultAutoFillPolicy))
                .ForMember(dest => dest.DefaultOverlapPolicy, opt => opt.MapFrom(src => (int)src.DefaultOverlapPolicy));

            #endregion

            #region Kaltura Playback Context

            cfg.CreateMap<ApiObjects.PlaybackAdapter.PlaybackContext, KalturaPlaybackContext>()
             .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => GetKalturaPlaybackContextActions(src.Actions)))
             .ForMember(dest => dest.Messages, opt => opt.MapFrom(src => src.Messages))
             .ForMember(dest => dest.Sources, opt => opt.MapFrom(src => src.Sources))
             .ForMember(dest => dest.Plugins, opt => opt.MapFrom(src => src.Plugins))
             .ForMember(dest => dest.PlaybackCaptions, opt => opt.MapFrom(src => src.PlaybackCaptions));

            cfg.CreateMap<KalturaPlaybackContext, ApiObjects.PlaybackAdapter.PlaybackContext>()
             .ForMember(dest => dest.Messages, opt => opt.MapFrom(src => src.Messages))
             .ForMember(dest => dest.Sources, opt => opt.MapFrom(src => src.Sources))
             .ForMember(dest => dest.Plugins, opt => opt.MapFrom(src => src.Plugins))
             .ForMember(dest => dest.PlaybackCaptions, opt => opt.MapFrom(src => src.PlaybackCaptions));

            cfg.CreateMap<ApiObjects.PlaybackAdapter.PlaybackSource, KalturaPlaybackSource>()
                .ForMember(dest => dest.Format, opt => opt.MapFrom(src => src.Format))
                .ForMember(dest => dest.Protocols, opt => opt.MapFrom(src => src.Protocols))
                .ForMember(dest => dest.Drm, opt => opt.MapFrom(src => src.Drm))
                .ForMember(dest => dest.AdsPolicy, opt => opt.MapFrom(src => src.AdsPolicy))
                .ForMember(dest => dest.AdsParams, opt => opt.MapFrom(src => src.AdsParams))
                .ForMember(dest => dest.FileExtention, opt => opt.MapFrom(src => ConvertPlaybackSourceFileExtention(src)))
                .ForMember(dest => dest.DrmId, opt => opt.MapFrom(src => src.DrmId))
                .ForMember(dest => dest.IsTokenized, opt => opt.MapFrom(src => src.IsTokenized))
                .ForMember(dest => dest.Opl, opt => opt.MapFrom(src => src.OutputProtecationLevel))
                .ForMember(dest => dest.BusinessModuleDetails, opt => opt.MapFrom(src => src.BusinessModuleDetails))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.Labels))
                .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => BuildStringAndKalturaStringValueArraySerializableDictionary(src.DynamicData)))
                ;

            cfg.CreateMap<KalturaPlaybackSource, ApiObjects.PlaybackAdapter.PlaybackSource>()
                .ForMember(dest => dest.Format, opt => opt.MapFrom(src => src.Format))
                .ForMember(dest => dest.Protocols, opt => opt.MapFrom(src => src.Protocols))
                .ForMember(dest => dest.Drm, opt => opt.MapFrom(src => src.Drm))
                .ForMember(dest => dest.AdsPolicy, opt => opt.MapFrom(src => src.AdsPolicy))
                .ForMember(dest => dest.AdsParams, opt => opt.MapFrom(src => src.AdsParams))
                .ForMember(dest => dest.FileExtention, opt => opt.MapFrom(src => src.FileExtention))
                .ForMember(dest => dest.DrmId, opt => opt.MapFrom(src => src.DrmId))
                .ForMember(dest => dest.IsTokenized, opt => opt.MapFrom(src => src.IsTokenized))
                .ForMember(dest => dest.OutputProtecationLevel, opt => opt.MapFrom(src => src.Opl))
                .ForMember(dest => dest.BusinessModuleDetails, opt => opt.MapFrom(src => src.BusinessModuleDetails))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.Labels))
                .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => BuildStringAndStringEnumerableDictionary(src.DynamicData)))
                ;

            cfg.CreateMap<ApiObjects.PlaybackAdapter.DrmPlaybackPluginData, KalturaPluginData>();

            cfg.CreateMap<KalturaPluginData, ApiObjects.PlaybackAdapter.DrmPlaybackPluginData>();

            cfg.CreateMap<ApiObjects.PlaybackAdapter.DrmPlaybackPluginData, KalturaDrmPlaybackPluginData>()
                .IncludeBase<ApiObjects.PlaybackAdapter.DrmPlaybackPluginData, KalturaPluginData>()
                .ForMember(dest => dest.Scheme, opt => opt.MapFrom(src => src.Scheme))
                .ForMember(dest => dest.LicenseURL, opt => opt.MapFrom(src => src.LicenseURL))
                .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => src.DynamicData != null ? src.DynamicData.ToDictionary(k => k.Key, v => v.Value) : null))
                ;

            cfg.CreateMap<KalturaDrmPlaybackPluginData, ApiObjects.PlaybackAdapter.DrmPlaybackPluginData>()
                .IncludeBase<KalturaPluginData, ApiObjects.PlaybackAdapter.DrmPlaybackPluginData>()
                .ForMember(dest => dest.Scheme, opt => opt.MapFrom(src => src.Scheme))
                .ForMember(dest => dest.LicenseURL, opt => opt.MapFrom(src => src.LicenseURL))
                .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => WebAPI.Utils.Utils.ConvertSerializeableDictionary(src.DynamicData, true, false)))
                .AfterMap((src, dest) => dest.DynamicData = src.DynamicData != null ? dest.DynamicData : null)
                ;

            cfg.CreateMap<ApiObjects.PlaybackAdapter.FairPlayPlaybackPluginData, KalturaFairPlayPlaybackPluginData>()
                .IncludeBase<ApiObjects.PlaybackAdapter.DrmPlaybackPluginData, KalturaDrmPlaybackPluginData>()
                .ForMember(dest => dest.Certificate, opt => opt.MapFrom(src => src.Certificate));

            cfg.CreateMap<KalturaFairPlayPlaybackPluginData, ApiObjects.PlaybackAdapter.FairPlayPlaybackPluginData>()
                .IncludeBase<KalturaDrmPlaybackPluginData, ApiObjects.PlaybackAdapter.DrmPlaybackPluginData>()
                .ForMember(dest => dest.Certificate, opt => opt.MapFrom(src => src.Certificate));

            cfg.CreateMap<ApiObjects.PlaybackAdapter.CustomDrmPlaybackPluginData, KalturaCustomDrmPlaybackPluginData>()
               .IncludeBase<ApiObjects.PlaybackAdapter.DrmPlaybackPluginData, KalturaDrmPlaybackPluginData>()
               .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data));

            cfg.CreateMap<KalturaCustomDrmPlaybackPluginData, ApiObjects.PlaybackAdapter.CustomDrmPlaybackPluginData>()
                .IncludeBase<KalturaDrmPlaybackPluginData, ApiObjects.PlaybackAdapter.DrmPlaybackPluginData>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data));

            cfg.CreateMap<ApiObjects.PlaybackAdapter.CaptionPlaybackPluginData, KalturaCaptionPlaybackPluginData>()
                .ForMember(dest => dest.Format, opt => opt.MapFrom(src => src.Format))
                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label))
                .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language))
                .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.URL));

            cfg.CreateMap<KalturaCaptionPlaybackPluginData, ApiObjects.PlaybackAdapter.CaptionPlaybackPluginData>()
                .ForMember(dest => dest.Format, opt => opt.MapFrom(src => src.Format))
                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label))
                .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language))
                .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.URL));

            cfg.CreateMap<ApiObjects.PlaybackAdapter.PlaybackPluginData, KalturaPlaybackPluginData>();

            cfg.CreateMap<KalturaPlaybackPluginData, ApiObjects.PlaybackAdapter.PlaybackPluginData>();

            cfg.CreateMap<ApiObjects.PlaybackAdapter.BumperPlaybackPluginData, KalturaBumpersPlaybackPluginData>()
                .IncludeBase<ApiObjects.PlaybackAdapter.PlaybackPluginData, KalturaPlaybackPluginData>()
                .ForMember(dest => dest.StreamerType, opt => opt.MapFrom(src => src.StreamerType))
                .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.URL));

            cfg.CreateMap<KalturaBumpersPlaybackPluginData, ApiObjects.PlaybackAdapter.BumperPlaybackPluginData>()
                .IncludeBase<KalturaPlaybackPluginData, ApiObjects.PlaybackAdapter.PlaybackPluginData>()
                .ForMember(dest => dest.StreamerType, opt => opt.MapFrom(src => src.StreamerType))
                .ForMember(dest => dest.URL, opt => opt.MapFrom(src => src.URL));

            cfg.CreateMap<ApiObjects.PlaybackAdapter.AccessControlMessage, KalturaAccessControlMessage>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message));

            cfg.CreateMap<KalturaAccessControlMessage, ApiObjects.PlaybackAdapter.AccessControlMessage>()
             .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
             .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message));

            cfg.CreateMap<ApiObjects.PlaybackAdapter.RuleAction, KalturaRuleAction>()
             .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
             .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            cfg.CreateMap<KalturaRuleAction, ApiObjects.PlaybackAdapter.RuleAction>()
             .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
             .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            cfg.CreateMap<KalturaRuleActionType, ApiObjects.PlaybackAdapter.RuleActionType>()
               .ConvertUsing(kalturaRuleActionType =>
               {
                   switch (kalturaRuleActionType)
                   {
                       case KalturaRuleActionType.BLOCK:
                           return ApiObjects.PlaybackAdapter.RuleActionType.BLOCK;
                           break;
                       case KalturaRuleActionType.START_DATE_OFFSET:
                           return ApiObjects.PlaybackAdapter.RuleActionType.START_DATE_OFFSET;
                           break;
                       case KalturaRuleActionType.END_DATE_OFFSET:
                           return ApiObjects.PlaybackAdapter.RuleActionType.END_DATE_OFFSET;
                           break;
                       case KalturaRuleActionType.USER_BLOCK:
                           return ApiObjects.PlaybackAdapter.RuleActionType.USER_BLOCK;
                           break;
                       case KalturaRuleActionType.ALLOW_PLAYBACK:
                           return ApiObjects.PlaybackAdapter.RuleActionType.ALLOW_PLAYBACK;
                           break;
                       case KalturaRuleActionType.BLOCK_PLAYBACK:
                           return ApiObjects.PlaybackAdapter.RuleActionType.BLOCK_PLAYBACK;
                           break;
                       case KalturaRuleActionType.APPLY_DISCOUNT_MODULE:
                           return ApiObjects.PlaybackAdapter.RuleActionType.APPLY_DISCOUNT_MODULE;
                           break;
                       case KalturaRuleActionType.APPLY_PLAYBACK_ADAPTER:
                           return ApiObjects.PlaybackAdapter.RuleActionType.APPLY_PLAYBACK_ADAPTER;
                           break;
                       case KalturaRuleActionType.FILTER:
                           return ApiObjects.PlaybackAdapter.RuleActionType.USER_FILTER;
                           break;
                       default:
                           throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown kalturaRuleActionType value : {0}", kalturaRuleActionType.ToString()));
                           break;
                   }
               });

            cfg.CreateMap<ApiObjects.PlaybackAdapter.RuleActionType, KalturaRuleActionType>()
                .ConvertUsing(ruleActionType =>
                {

                    switch (ruleActionType)
                    {
                        case ApiObjects.PlaybackAdapter.RuleActionType.BLOCK:
                            return KalturaRuleActionType.BLOCK;
                            break;
                        case ApiObjects.PlaybackAdapter.RuleActionType.START_DATE_OFFSET:
                            return KalturaRuleActionType.START_DATE_OFFSET;
                            break;
                        case ApiObjects.PlaybackAdapter.RuleActionType.END_DATE_OFFSET:
                            return KalturaRuleActionType.END_DATE_OFFSET;
                            break;
                        case ApiObjects.PlaybackAdapter.RuleActionType.USER_BLOCK:
                            return KalturaRuleActionType.USER_BLOCK;
                            break;
                        case ApiObjects.PlaybackAdapter.RuleActionType.ALLOW_PLAYBACK:
                            return KalturaRuleActionType.ALLOW_PLAYBACK;
                            break;
                        case ApiObjects.PlaybackAdapter.RuleActionType.BLOCK_PLAYBACK:
                            return KalturaRuleActionType.BLOCK_PLAYBACK;
                            break;
                        case ApiObjects.PlaybackAdapter.RuleActionType.APPLY_DISCOUNT_MODULE:
                            return KalturaRuleActionType.APPLY_DISCOUNT_MODULE;
                            break;
                        case ApiObjects.PlaybackAdapter.RuleActionType.APPLY_PLAYBACK_ADAPTER:
                            return KalturaRuleActionType.APPLY_PLAYBACK_ADAPTER;
                            break;
                        case ApiObjects.PlaybackAdapter.RuleActionType.USER_FILTER:
                            return KalturaRuleActionType.FILTER;
                            break;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown PlaybackAdapterRuleActionType value : {0}", ruleActionType.ToString()));
                            break;
                    }
                });

            cfg.CreateMap<KalturaAdsPolicy, ApiObjects.PlaybackAdapter.AdsPolicy>()
               .ConvertUsing(kalturaType =>
               {
                   switch (kalturaType)
                   {
                       case KalturaAdsPolicy.KEEP_ADS:
                           return ApiObjects.PlaybackAdapter.AdsPolicy.KEEP_ADS;
                           break;
                       case KalturaAdsPolicy.NO_ADS:
                           return ApiObjects.PlaybackAdapter.AdsPolicy.NO_ADS;
                           break;
                       default:
                           throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown KalturaAdsPolicy value : {0}", kalturaType.ToString()));
                           break;
                   }
               });

            cfg.CreateMap<ApiObjects.PlaybackAdapter.AdsPolicy, KalturaAdsPolicy>()
                .ConvertUsing(type =>
                {

                    switch (type)
                    {
                        case ApiObjects.PlaybackAdapter.AdsPolicy.KEEP_ADS:
                            return KalturaAdsPolicy.KEEP_ADS;
                            break;
                        case ApiObjects.PlaybackAdapter.AdsPolicy.NO_ADS:
                            return KalturaAdsPolicy.NO_ADS;
                            break;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown PlaybackAdapterAdsPolicy value : {0}", type.ToString()));
                            break;
                    }
                });

            cfg.CreateMap<KalturaDrmSchemeName, ApiObjects.PlaybackAdapter.DrmSchemeName>()
                .ConvertUsing(kalturaType =>
                {
                    switch (kalturaType)

                    {
                        case KalturaDrmSchemeName.PLAYREADY_CENC:
                            return ApiObjects.PlaybackAdapter.DrmSchemeName.PLAYREADY_CENC;
                            break;
                        case KalturaDrmSchemeName.WIDEVINE_CENC:
                            return ApiObjects.PlaybackAdapter.DrmSchemeName.WIDEVINE_CENC;
                            break;
                        case KalturaDrmSchemeName.FAIRPLAY:
                            return ApiObjects.PlaybackAdapter.DrmSchemeName.FAIRPLAY;
                            break;
                        case KalturaDrmSchemeName.WIDEVINE:
                            return ApiObjects.PlaybackAdapter.DrmSchemeName.WIDEVINE;
                            break;
                        case KalturaDrmSchemeName.PLAYREADY:
                            return ApiObjects.PlaybackAdapter.DrmSchemeName.PLAYREADY;
                            break;
                        case KalturaDrmSchemeName.CUSTOM_DRM:
                            return ApiObjects.PlaybackAdapter.DrmSchemeName.CUSTOM_DRM;
                            break;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown KalturaDrmSchemeName value : {0}", kalturaType.ToString()));
                            break;
                    }
                });


            cfg.CreateMap<ApiObjects.PlaybackAdapter.DrmSchemeName, KalturaDrmSchemeName>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case ApiObjects.PlaybackAdapter.DrmSchemeName.PLAYREADY_CENC:
                            return KalturaDrmSchemeName.PLAYREADY_CENC;
                            break;
                        case ApiObjects.PlaybackAdapter.DrmSchemeName.WIDEVINE_CENC:
                            return KalturaDrmSchemeName.WIDEVINE_CENC;
                            break;
                        case ApiObjects.PlaybackAdapter.DrmSchemeName.FAIRPLAY:
                            return KalturaDrmSchemeName.FAIRPLAY;
                            break;
                        case ApiObjects.PlaybackAdapter.DrmSchemeName.WIDEVINE:
                            return KalturaDrmSchemeName.WIDEVINE;
                            break;
                        case ApiObjects.PlaybackAdapter.DrmSchemeName.PLAYREADY:
                            return KalturaDrmSchemeName.PLAYREADY;
                            break;
                        case ApiObjects.PlaybackAdapter.DrmSchemeName.CUSTOM_DRM:
                            return KalturaDrmSchemeName.CUSTOM_DRM;
                            break;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown DrmSchemeName value : {0}", type.ToString()));
                            break;
                    }
                });

            cfg.CreateMap<RuleStatus, KalturaAssetRuleStatus>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case RuleStatus.InProgress:
                            return KalturaAssetRuleStatus.IN_PROGRESS;
                            break;
                        case RuleStatus.Ready:
                            return KalturaAssetRuleStatus.READY;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown RuleStatus value : {0}", type.ToString()));
                            break;
                    }
                });

            cfg.CreateMap<KalturaPlaybackContextOptions, ApiObjects.PlaybackAdapter.RequestPlaybackContextOptions>()
            .ForMember(dest => dest.AdapterData, opt => opt.MapFrom(src => WebAPI.Utils.Utils.ConvertSerializeableDictionary(src.AdapterData, false, false)))
            .ForMember(dest => dest.AssetFileIds, opt => opt.MapFrom(src => src.AssetFileIds))
            .ForMember(dest => dest.Context, opt => opt.MapFrom(src => src.Context))
            .ForMember(dest => dest.UrlType, opt => opt.MapFrom(src => src.UrlType));

            cfg.CreateMap<KalturaPlaybackContextType, ApiObjects.PlayContextType>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case KalturaPlaybackContextType.CATCHUP:
                            return ApiObjects.PlayContextType.CatchUp;
                        case KalturaPlaybackContextType.DOWNLOAD:
                            return ApiObjects.PlayContextType.Download;
                        case KalturaPlaybackContextType.PLAYBACK:
                            return ApiObjects.PlayContextType.Playback;
                        case KalturaPlaybackContextType.START_OVER:
                            return ApiObjects.PlayContextType.StartOver;
                        case KalturaPlaybackContextType.TRAILER:
                            return ApiObjects.PlayContextType.Trailer;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown PlayContextType value : {0}", type.ToString()));
                    }
                });

            cfg.CreateMap<KalturaUrlType, ApiObjects.UrlType>()
               .ConvertUsing(type =>
               {
                   switch (type)
                   {
                       case KalturaUrlType.DIRECT:
                           return ApiObjects.UrlType.direct;
                       case KalturaUrlType.PLAYMANIFEST:
                           return ApiObjects.UrlType.playmanifest;
                       default:
                           throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown UrlType value : {0}", type.ToString()));
                   }
               });

            #endregion

            #region EventNotification

            //EventNotificationAction, KalturaEventNotification
            cfg.CreateMap<EventNotificationAction, KalturaEventNotification>()
                .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => src.ActionType))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.ObjectId, opt => opt.MapFrom(src => src.ObjectId))
                .ForMember(dest => dest.EventObjectType, opt => opt.MapFrom(src => src.ObjectType))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                ;

            cfg.CreateMap<KalturaEventNotification, EventNotificationAction>()
                .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => src.ActionType))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.ObjectId, opt => opt.MapFrom(src => src.ObjectId))
                .ForMember(dest => dest.ObjectType, opt => opt.MapFrom(src => src.EventObjectType))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                ;

            cfg.CreateMap<KalturaEventNotificationFilter, EventNotificationActionFilter>()
                .ForMember(dest => dest.ObjectId, opt => opt.MapFrom(src => src.ObjectIdEqual))
                .ForMember(dest => dest.ObjectType, opt => opt.MapFrom(src => src.EventObjectTypeEqual))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IdEqual))
                ;

            cfg.CreateMap<EventNotificationActionStatus, KalturaEventNotificationStatus>()
                .ConvertUsing(eventNotificationActionStatus =>
                {
                    switch (eventNotificationActionStatus)
                    {
                        case EventNotificationActionStatus.Failed:
                            return KalturaEventNotificationStatus.FAILED;
                        case EventNotificationActionStatus.FailedToSend:
                            return KalturaEventNotificationStatus.FAILED_TO_SEND;
                        case EventNotificationActionStatus.Sent:
                            return KalturaEventNotificationStatus.SENT;
                        case EventNotificationActionStatus.Success:
                            return KalturaEventNotificationStatus.SUCCESS;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown EventNotificationActionStatus value : {eventNotificationActionStatus.ToString()}");
                    }
                });

            cfg.CreateMap<KalturaEventNotificationStatus, EventNotificationActionStatus>()
                .ConvertUsing(eventNotificationActionStatus =>
                {
                    switch (eventNotificationActionStatus)
                    {
                        case KalturaEventNotificationStatus.FAILED:
                            return EventNotificationActionStatus.Failed;
                        case KalturaEventNotificationStatus.FAILED_TO_SEND:
                            return EventNotificationActionStatus.FailedToSend;
                        case KalturaEventNotificationStatus.SENT:
                            return EventNotificationActionStatus.Sent;
                        case KalturaEventNotificationStatus.SUCCESS:
                            return EventNotificationActionStatus.Success;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown KalturaEventNotificationStatus value : {eventNotificationActionStatus.ToString()}");
                    }
                });

            //EventNotificationScope, KalturaEventNotificationScope
            cfg.CreateMap<KalturaEventNotificationScope, EventNotificationScope>();

            cfg.CreateMap<KalturaEventNotificationObjectScope, EventNotificationObjectScope>()
                .IncludeBase<KalturaEventNotificationScope, EventNotificationScope>()
                .ForMember(dest => dest.EventObject, opt => opt.MapFrom(src => src.EventObject));

            #endregion

            cfg.CreateMap<KalturaDynamicList, DynamicList>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            cfg.CreateMap<DynamicList, KalturaDynamicList>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            cfg.CreateMap<KalturaUdidDynamicList, UdidDynamicList>()
                .IncludeBase<KalturaDynamicList, DynamicList>();

            cfg.CreateMap<UdidDynamicList, KalturaUdidDynamicList>()
                .IncludeBase<DynamicList, KalturaDynamicList>();

            cfg.CreateMap<KalturaDynamicListFilter, DynamicListFilter>();

            cfg.CreateMap<KalturaDynamicListIdInFilter, DynamicListnIdInFilter>()
                .IncludeBase<KalturaDynamicListFilter, DynamicListFilter>()
                .ForMember(dest => dest.IdIn, opt => opt.ResolveUsing(src => !string.IsNullOrEmpty(src.IdIn) ? WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(src.IdIn, "filter.idIn") : null));

            cfg.CreateMap<KalturaDynamicListSearchFilter, DynamicListSearchFilter>()
                .IncludeBase<KalturaDynamicListFilter, DynamicListFilter>()
                .ForMember(dest => dest.IdEqual, opt => opt.MapFrom(src => src.IdEqual))
                .ForMember(dest => dest.ValueEqual, opt => opt.MapFrom(src => src.ValueEqual));

            cfg.CreateMap<KalturaStringValueArray, List<string>>().ConvertUsing(v => v.Objects.Select(_ => _.value).ToList());
            cfg.CreateMap<List<string>, KalturaStringValueArray>().ConvertUsing(v => new KalturaStringValueArray
                { Objects = v.Select(_ => new KalturaStringValue { value = _ }).ToList() });
        }

        private static string GetStreamerTypes(List<StreamerType> types)
        {
            var mapped = AutoMapper.Mapper.Map<List<KalturaMediaFileStreamerType>>(types);
            return string.Join(",", mapped);
        }

        private static string GetQualities(List<MediaFileTypeQuality> types)
        {
            var mapped = AutoMapper.Mapper.Map<List<KalturaMediaFileTypeQuality>>(types);
            return string.Join(",", mapped);
        }

        private static string GetAssetTypes(List<eAssetTypes> types)
        {
            var mapped = AutoMapper.Mapper.Map<List<KalturaAssetType>>(types);
            return string.Join(",", mapped);
        }

        #region Private Convertors
        private static KalturaCampaignOrderBy ConvertCampaignOrder(CampaignOrderBy? order)
        {
            switch (order)
            {
                case CampaignOrderBy.StartDateDesc:
                    return KalturaCampaignOrderBy.START_DATE_DESC;
                case CampaignOrderBy.StartDateAsc:
                    return KalturaCampaignOrderBy.START_DATE_ASC ;
                case CampaignOrderBy.UpdateDateAsc:
                    return KalturaCampaignOrderBy.UPDATE_DATE_ASC;
                case CampaignOrderBy.UpdateDateDesc:
                    return KalturaCampaignOrderBy.UPDATE_DATE_DESC;
                case CampaignOrderBy.EndDateAsc:
                    return KalturaCampaignOrderBy.END_DATE_ASC;
                case CampaignOrderBy.EndDateDesc:
                    return KalturaCampaignOrderBy.END_DATE_DESC;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown Campaign Order By");
            }
        }

        private static CampaignOrderBy ConvertCampaignOrder(KalturaCampaignOrderBy? order)
        {
            switch (order)
            {
                case KalturaCampaignOrderBy.START_DATE_DESC:
                    return CampaignOrderBy.StartDateDesc;
                case KalturaCampaignOrderBy.START_DATE_ASC:
                    return CampaignOrderBy.StartDateAsc;
                case KalturaCampaignOrderBy.UPDATE_DATE_ASC:
                    return CampaignOrderBy.UpdateDateAsc;
                case KalturaCampaignOrderBy.UPDATE_DATE_DESC:
                    return CampaignOrderBy.UpdateDateDesc;
                case KalturaCampaignOrderBy.END_DATE_ASC:
                    return CampaignOrderBy.EndDateAsc;
                case KalturaCampaignOrderBy.END_DATE_DESC:
                    return CampaignOrderBy.EndDateDesc;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown Campaign Order By");
            }
        }

        private static KalturaObjectState ConvertObjectState(CampaignState? state)
        {
            switch (state)
            {
                case CampaignState.ACTIVE:
                    return KalturaObjectState.ACTIVE;
                case CampaignState.ARCHIVE:
                    return KalturaObjectState.ARCHIVE;
                case CampaignState.INACTIVE:
                    return KalturaObjectState.INACTIVE;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown Object State");

            }
        }

        private static CampaignState ConvertObjectState(KalturaObjectState? state)
        {
            switch (state)
            {
                case KalturaObjectState.ACTIVE:
                    return CampaignState.ACTIVE;
                case KalturaObjectState.ARCHIVE:
                    return CampaignState.ARCHIVE;
                case KalturaObjectState.INACTIVE:
                    return CampaignState.INACTIVE;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown Object State");

            }
        }

        internal static StatsType ConvertAssetTypeToStatsType(AssetType type)
        {
            StatsType result;
            switch (type)
            {
                case AssetType.media:
                    result = StatsType.MEDIA;
                    break;
                case AssetType.epg:
                    result = StatsType.EPG;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown AssetType");
            }

            return result;
        }

        private static long GetConvertedIp(string ip)
        {
            APILogic.Utils.ConvertIpToNumber(ip, out var convertIp, out var isV6);
            return long.Parse(convertIp);
        }

        private static List<Permission> ConvertPermissionsNames(string permissionNames, string excludedPermissionNames)
        {
            List<Permission> result = new List<Permission>();
            HashSet<string> duplicatePermission = new HashSet<string>();

            if (!string.IsNullOrEmpty(permissionNames))
            {
                foreach (string permission in permissionNames.Split(','))
                {
                    if (!duplicatePermission.Contains(permission.ToLower()))
                    {
                        duplicatePermission.Add(permission.ToLower());
                        result.Add(new Permission() { isExcluded = false, Name = permission });
                    }
                }
            }
            if (!string.IsNullOrEmpty(excludedPermissionNames))
            {
                foreach (string permission in excludedPermissionNames.Split(','))
                {
                    if (!duplicatePermission.Contains(permission.ToLower()))
                    {
                        duplicatePermission.Add(permission.ToLower());
                        result.Add(new Permission() { isExcluded = true, Name = permission });
                    }
                    else
                    {
                        result.Remove(result.Where(x => x.Name.ToLower() == permission.ToLower()).FirstOrDefault());
                    }
                }
            }
            return result;
        }

        private static object ConvertPermissionsNames(List<Permission> permissions, bool isExcluded)
        {
            string result = null;

            if (permissions != null && permissions.Count > 0)
            {
                result = string.Join(",", permissions.Where(x => x.isExcluded == isExcluded).Select(x => x.Name).ToList());
            }

            return result;
        }

        private static string ConvertAssetGroupByToGroupBy(KalturaAssetGroupBy groupBy)
        {
            if (groupBy == null)
            {
                return string.Empty;
            }
            return groupBy.GetValue();
        }

        private static KalturaAssetGroupBy ConvertGroupByToAssetGroupBy(string groupBy)
        {
            KalturaAssetGroupBy kalturaAssetGroupBy;

            if (Enum.IsDefined(typeof(KalturaGroupByField), groupBy))
            {
                kalturaAssetGroupBy = new KalturaAssetFieldGroupBy();
                KalturaGroupByField groupByField = (KalturaGroupByField)Enum.Parse(typeof(KalturaGroupByField), groupBy);

                ((KalturaAssetFieldGroupBy)kalturaAssetGroupBy).Value = groupByField;

            }
            else
            {
                kalturaAssetGroupBy = new KalturaAssetMetaOrTagGroupBy();
                ((KalturaAssetMetaOrTagGroupBy)kalturaAssetGroupBy).Value = groupBy;
            }
            return kalturaAssetGroupBy;
        }

        private static bool ConvertIsTag(KalturaMetaType kalturaMetaType)
        {
            if (kalturaMetaType == KalturaMetaType.STRING_ARRAY)
            {
                return true;
            }

            return false;
        }

        private static List<MetaFeatureType> ConvertFeatures(string metaFeatureType)
        {
            List<MetaFeatureType> featureList = null;

            if (!string.IsNullOrEmpty(metaFeatureType))
            {
                featureList = new List<MetaFeatureType>();
                string[] metaFeatures = metaFeatureType.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string feature in metaFeatures)
                {
                    KalturaMetaFeatureType kalturaMetaFeatureType;
                    if (Enum.TryParse<KalturaMetaFeatureType>(feature.ToUpper(), out kalturaMetaFeatureType))
                    {
                        featureList.Add(ConvertMetaFeatureType(kalturaMetaFeatureType));
                    }
                }
            }

            return featureList;
        }

        private static string ConvertFeatures(List<MetaFeatureType> list)
        {
            string metaFeatureType = string.Empty;
            List<KalturaMetaFeatureType> KalturaMetaFeatureTypeList;
            if (list != null)
            {
                KalturaMetaFeatureTypeList = new List<KalturaMetaFeatureType>();
                foreach (MetaFeatureType featureType in list)
                {
                    KalturaMetaFeatureTypeList.Add(ConvertMetaFeatureType(featureType));
                }

                metaFeatureType = string.Join(",", KalturaMetaFeatureTypeList.ToArray());
            }

            return metaFeatureType;
        }

        private static MetaFeatureType ConvertMetaFeatureType(KalturaMetaFeatureType kalturaMetaFeatureType)
        {
            switch (kalturaMetaFeatureType)
            {
                case KalturaMetaFeatureType.USER_INTEREST:
                    return MetaFeatureType.USER_INTEREST;
                case KalturaMetaFeatureType.ENABLED_NOTIFICATION:
                    return MetaFeatureType.ENABLED_NOTIFICATION;
                default:
                    throw new ClientException((int)StatusCode.Error, string.Format("Unknown metaFeatureType value : {0}", kalturaMetaFeatureType.ToString()));
            }
        }

        private static KalturaMetaFeatureType ConvertMetaFeatureType(MetaFeatureType featureType)
        {
            switch (featureType)
            {
                case MetaFeatureType.USER_INTEREST:
                    return KalturaMetaFeatureType.USER_INTEREST;
                case MetaFeatureType.ENABLED_NOTIFICATION:
                    return KalturaMetaFeatureType.ENABLED_NOTIFICATION;
                default:
                    throw new ClientException((int)StatusCode.Error, string.Format("Unknown metaFeatureType value : {0}", featureType.ToString()));
            }
        }

        public static List<MetaFeatureType> ConvertMetaFeatureTypes(List<KalturaMetaFeatureType> kalturaMetaFeatureTypeList)
        {
            List<MetaFeatureType> list = new List<MetaFeatureType>();
            if (kalturaMetaFeatureTypeList != null)
            {
                foreach (KalturaMetaFeatureType kalturaMetaFeatureType in kalturaMetaFeatureTypeList)
                {
                    list.Add(ConvertMetaFeatureType(kalturaMetaFeatureType));
                }
            }

            return list;
        }

        private static ProtectionPolicy? ConvertProtectionPolicy(KalturaProtectionPolicy? protectionPolicy)
        {
            ProtectionPolicy? result = null;

            if (protectionPolicy.HasValue)
            {
                switch (protectionPolicy)
                {
                    case KalturaProtectionPolicy.ExtendingRecordingLifetime:
                        result = ProtectionPolicy.ExtendingRecordingLifetime;
                        break;
                    case KalturaProtectionPolicy.LimitedByRecordingLifetime:
                        result = ProtectionPolicy.LimitedByRecordingLifetime;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown protection policy value");
                }
            }
            return result;
        }

        private static KalturaProtectionPolicy? ConvertProtectionPolicy(ProtectionPolicy? protectionPolicy)
        {
            KalturaProtectionPolicy? result = null;

            if (protectionPolicy.HasValue)
            {
                switch (protectionPolicy)
                {
                    case ProtectionPolicy.ExtendingRecordingLifetime:
                        result = KalturaProtectionPolicy.ExtendingRecordingLifetime;
                        break;
                    case ProtectionPolicy.LimitedByRecordingLifetime:
                        result = KalturaProtectionPolicy.LimitedByRecordingLifetime;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown protection policy value");
                }
            }
            return result;
        }

        private static KalturaQuotaOveragePolicy? ConvertQuotaOveragePolicy(QuotaOveragePolicy? quotaOveragePolicy)
        {
            KalturaQuotaOveragePolicy? result = null;

            if (quotaOveragePolicy.HasValue)
            {
                switch (quotaOveragePolicy)
                {
                    case QuotaOveragePolicy.FIFOAutoDelete:
                        result = KalturaQuotaOveragePolicy.FIFOAutoDelete;
                        break;
                    case QuotaOveragePolicy.StopAtQuota:
                        result = KalturaQuotaOveragePolicy.StopAtQuota;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown quota overage policy value");
                }
            }
            return result;
        }

        private static QuotaOveragePolicy? ConvertQuotaOveragePolicy(KalturaQuotaOveragePolicy? kalturaQuotaOveragePolicy)
        {
            QuotaOveragePolicy? result = null;

            if (kalturaQuotaOveragePolicy.HasValue)
            {
                switch (kalturaQuotaOveragePolicy)
                {
                    case KalturaQuotaOveragePolicy.FIFOAutoDelete:
                        result = QuotaOveragePolicy.FIFOAutoDelete;
                        break;
                    case KalturaQuotaOveragePolicy.StopAtQuota:
                        result = QuotaOveragePolicy.StopAtQuota;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown quota overage policy value");
                }
            }
            return result;
        }

        private static KalturaApiParameterPermissionItemAction ConvertApiParameterPermissionItemAction(string action)
        {
            return (KalturaApiParameterPermissionItemAction)Enum.Parse(typeof(KalturaApiParameterPermissionItemAction), action);
        }

        private static List<KalturaPermissionItem> ConvertPermissionItems(List<PermissionItem> permissionItems)
        {
            List<KalturaPermissionItem> result = null;

            if (permissionItems != null && permissionItems.Count > 0)
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
                    else if (permissionItem is ApiPriviligesPermissionItem)
                    {
                        item = AutoMapper.Mapper.Map<KalturaApiPriviligesPermissionItem>((ApiPriviligesPermissionItem)permissionItem);
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

        private static List<KalturaPermission> ConvertPermissions(List<Permission> permissions)
        {
            List<KalturaPermission> result = null;

            if (permissions != null && permissions.Count > 0)
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

        private static List<KalturaChannelEnrichmentHolder> ConvertEnrichments(List<ExternalRecommendationEngineEnrichment> list)
        {
            List<KalturaChannelEnrichmentHolder> result = null;

            if (list != null && list.Count > 0)
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
                //case ExternalRecommendationEngineEnrichment.AtHome:
                //    result = new KalturaChannelEnrichmentHolder() { type = KalturaChannelEnrichment.AtHome };
                //    break;
                //case ExternalRecommendationEngineEnrichment.Catchup:
                //    result = new KalturaChannelEnrichmentHolder() { type = KalturaChannelEnrichment.Catchup };
                //    break;
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
                //case ExternalRecommendationEngineEnrichment.NPVRSupport:
                //    result = new KalturaChannelEnrichmentHolder() { type = KalturaChannelEnrichment.NPVRSupport };
                //    break;
                //case ExternalRecommendationEngineEnrichment.Parental:
                //    result = new KalturaChannelEnrichmentHolder() { type = KalturaChannelEnrichment.Parental };
                //    break;
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
                //case KalturaChannelEnrichment.AtHome:
                //    result = ExternalRecommendationEngineEnrichment.AtHome;
                //    break;
                //case KalturaChannelEnrichment.Catchup:
                //    result = ExternalRecommendationEngineEnrichment.Catchup;
                //    break;
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
                    //case KalturaChannelEnrichment.NPVRSupport:
                    //    result = ExternalRecommendationEngineEnrichment.NPVRSupport;
                    //    break;
                    //case KalturaChannelEnrichment.Parental:
                    //    result = ExternalRecommendationEngineEnrichment.Parental;
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

        internal static Dictionary<string, KalturaStringValue> ConvertRecommendationEngineSettings(List<RecommendationEngineSettings> settings)
        {
            if (settings == null)
                return null;

            Dictionary<string, KalturaStringValue> result = null;

            if (settings.Count > 0)
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

        internal static List<RecommendationEngineSettings> ConvertRecommendationEngineSettings(SerializableDictionary<string, KalturaStringValue> settings)
        {
            if (settings == null)
                return null;

            List<RecommendationEngineSettings> result = null;

            if (settings.Count > 0)
            {
                result = new List<RecommendationEngineSettings>();
                RecommendationEngineSettings pc;
                foreach (KeyValuePair<string, KalturaStringValue> data in settings)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        pc = new RecommendationEngineSettings();
                        pc.key = data.Key;
                        pc.value = data.Value.value;
                        result.Add(pc);
                    }
                }
            }
            if (result != null && result.Count > 0)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        private static WebAPI.Models.API.KalturaParentalRuleType ConvertParentalRuleType(eParentalRuleType type)
        {
            WebAPI.Models.API.KalturaParentalRuleType result = WebAPI.Models.API.KalturaParentalRuleType.ALL;

            switch (type)
            {
                case eParentalRuleType.All:
                    result = WebAPI.Models.API.KalturaParentalRuleType.ALL;
                    break;
                case eParentalRuleType.Movies:
                    result = WebAPI.Models.API.KalturaParentalRuleType.MOVIES;
                    break;
                case eParentalRuleType.TVSeries:
                    result = WebAPI.Models.API.KalturaParentalRuleType.TV_SERIES;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown parental rule type");
            }

            return result;
        }

        private static eParentalRuleType? ConvertParentalRuleType(KalturaParentalRuleType? ruleType)
        {
            eParentalRuleType? result = null;
            if (ruleType.HasValue)
            {
                switch (ruleType.Value)
                {
                    case KalturaParentalRuleType.ALL:
                        result = eParentalRuleType.All;
                        break;
                    case KalturaParentalRuleType.MOVIES:
                        result = eParentalRuleType.Movies;
                        break;
                    case KalturaParentalRuleType.TV_SERIES:
                        result = eParentalRuleType.TVSeries;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown KalturaParentalRuleType");
                }
            }

            return result;
        }

        private static WebAPI.Models.API.KalturaRuleLevel ConvertRuleLevel(eRuleLevel? type)
        {
            WebAPI.Models.API.KalturaRuleLevel result = WebAPI.Models.API.KalturaRuleLevel.invalid;

            switch (type)
            {
                case eRuleLevel.User:
                    result = WebAPI.Models.API.KalturaRuleLevel.user;
                    break;
                case eRuleLevel.Domain:
                    result = WebAPI.Models.API.KalturaRuleLevel.household;
                    break;
                case eRuleLevel.Group:
                    result = WebAPI.Models.API.KalturaRuleLevel.account;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown rule level");

            }

            return result;
        }

        private static WebAPI.Models.API.KalturaPurchaseSettingsType? ConvertPurchaseSetting(ePurchaeSettingsType? type)
        {
            WebAPI.Models.API.KalturaPurchaseSettingsType result = WebAPI.Models.API.KalturaPurchaseSettingsType.block;

            if (type == null)
            {
                return null;
            }

            switch (type)
            {
                case ePurchaeSettingsType.Allow:
                    result = WebAPI.Models.API.KalturaPurchaseSettingsType.allow;
                    break;
                case ePurchaeSettingsType.Ask:
                    result = WebAPI.Models.API.KalturaPurchaseSettingsType.ask;
                    break;
                case ePurchaeSettingsType.Block:
                    result = WebAPI.Models.API.KalturaPurchaseSettingsType.block;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown purchase setting");

            }

            return result;
        }

        private static WebAPI.Models.API.KalturaRuleType ConvertRuleType(RuleType type)
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
                case RuleType.AssetUser:
                    result = WebAPI.Models.API.KalturaRuleType.assetUser;
                    break;
                case RuleType.Network:
                    result = WebAPI.Models.API.KalturaRuleType.network;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown rule type");
            }

            return result;
        }

        internal static Dictionary<string, int> ConvertErrorsDictionary(List<KeyValuePair> errors)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            foreach (KeyValuePair item in errors)
            {
                if (!result.ContainsKey(item.key))
                {
                    result.Add(item.key, int.Parse(item.value));
                }
            }

            return result;
        }

        internal static List<OSSAdapterSettings> ConvertOSSAdapterSettings(SerializableDictionary<string, KalturaStringValue> settings)
        {
            List<OSSAdapterSettings> result = null;

            if (settings != null && settings.Count > 0)
            {
                result = new List<OSSAdapterSettings>();
                OSSAdapterSettings pc;
                foreach (KeyValuePair<string, KalturaStringValue> data in settings)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        pc = new OSSAdapterSettings();
                        pc.key = data.Key;
                        pc.value = data.Value.value;
                        result.Add(pc);
                    }
                }
            }
            if (result != null && result.Count > 0)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static Dictionary<string, KalturaStringValue> ConvertOSSAdapterSettings(List<OSSAdapterSettings> settings)
        {
            Dictionary<string, KalturaStringValue> result = null;

            if (settings != null && settings.Count > 0)
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

        private static WebAPI.Models.API.KalturaExportDataType ConvertExportDataType(eBulkExportDataType type)
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

        private static WebAPI.Models.API.KalturaExportType ConvertExportType(eBulkExportExportType type)
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

        public static CDNAdapterSettings[] ConvertCDNAdapterSettings(SerializableDictionary<string, KalturaStringValue> settings)
        {
            List<CDNAdapterSettings> result = null;

            if (settings != null && settings.Count > 0)
            {
                result = new List<CDNAdapterSettings>();
                CDNAdapterSettings pc;
                foreach (KeyValuePair<string, KalturaStringValue> data in settings)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        pc = new CDNAdapterSettings();
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

        public static Dictionary<string, KalturaStringValue> ConvertCDNAdapterSettings(List<CDNAdapterSettings> settings)
        {
            Dictionary<string, KalturaStringValue> result = null;

            if (settings != null && settings.Count > 0)
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

        private static KalturaMetaType ConvertMetaType(ApiObjects.MetaType metaType)
        {
            KalturaMetaType response;

            switch (metaType)
            {
                case ApiObjects.MetaType.String:
                    response = KalturaMetaType.STRING;
                    break;
                case ApiObjects.MetaType.Number:
                    response = KalturaMetaType.NUMBER;
                    break;
                case ApiObjects.MetaType.Bool:
                    response = KalturaMetaType.BOOLEAN;
                    break;
                case ApiObjects.MetaType.Tag:
                    response = KalturaMetaType.STRING_ARRAY;
                    break;
                case ApiObjects.MetaType.DateTime:
                    response = KalturaMetaType.DATE;
                    break;
                case ApiObjects.MetaType.ReleatedEntity:
                    response = KalturaMetaType.RELEATED_ENTITY;
                    break;
                case ApiObjects.MetaType.All:
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

        internal static ApiObjects.MetaType ConvertMetaType(KalturaMetaType? metaType)
        {
            ApiObjects.MetaType response;

            if (metaType.HasValue)
            {
                switch (metaType.Value)
                {
                    case KalturaMetaType.STRING:
                        response = ApiObjects.MetaType.String;
                        break;
                    case KalturaMetaType.NUMBER:
                        response = ApiObjects.MetaType.Number;
                        break;
                    case KalturaMetaType.BOOLEAN:
                        response = ApiObjects.MetaType.Bool;
                        break;
                    case KalturaMetaType.STRING_ARRAY:
                        response = ApiObjects.MetaType.Tag;
                        break;
                    case KalturaMetaType.DATE:
                        response = ApiObjects.MetaType.DateTime;
                        break;
                    case KalturaMetaType.RELEATED_ENTITY:
                        response = ApiObjects.MetaType.ReleatedEntity;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown meta type");
                }
            }
            else
            {
                response = ApiObjects.MetaType.All;
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

        private static KalturaPermissionType ConvertPermissionType(ePermissionType type)
        {
            KalturaPermissionType response;

            switch (type)
            {
                case ePermissionType.Normal:
                    response = KalturaPermissionType.NORMAL;
                    break;
                case ePermissionType.Group:
                    response = KalturaPermissionType.GROUP;
                    break;
                case ePermissionType.SpecialFeature:
                    response = KalturaPermissionType.SPECIAL_FEATURE;
                    break;
                default:
                    response = KalturaPermissionType.NORMAL;
                    break;

                    //throw new ClientException((int)StatusCode.Error, "Unknown permission type");
            }

            return response;
        }

        private static ePermissionType ConvertPermissionType(KalturaPermissionType type)
        {
            ePermissionType response;

            switch (type)
            {
                case KalturaPermissionType.NORMAL:
                    response = ePermissionType.Normal;
                    break;
                case KalturaPermissionType.GROUP:
                    response = ePermissionType.Group;
                    break;
                case KalturaPermissionType.SPECIAL_FEATURE:
                    response = ePermissionType.SpecialFeature;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown permission type");
            }

            return response;
        }

        private static string ConvertPlaybackSourceFileExtention(ApiObjects.PlaybackAdapter.PlaybackSource source)
        {
            string response = string.Empty;

            if (string.IsNullOrEmpty(source.FileExtention))
            {
                try
                {
                    Uri uri = new Uri(source.Url);
                    response = uri.Segments.LastOrDefault();

                    if (!string.IsNullOrEmpty(response) && response.Contains("."))
                    {
                        response = response.Substring(response.LastIndexOf("."));
                    }
                }
                catch
                {
                }
            }

            return response;
        }

        private static List<KalturaRuleAction> GetKalturaPlaybackContextActions(List<ApiObjects.PlaybackAdapter.RuleAction> actions)
        {
            if (actions == null || actions.Count == 0) { return null; }

            var kalturaActionRules = new List<KalturaRuleAction>(actions.Select(x => new KalturaAccessControlBlockAction() { Description = x.Description }));
            return kalturaActionRules;
        }

        private static RoleProfileType? ConvertRoleProfileType(KalturaUserRoleProfile? profile)
        {
            RoleProfileType? res;
            switch (profile)
            {
                case KalturaUserRoleProfile.SYSTEM:
                    res = RoleProfileType.System;
                    break;
                case KalturaUserRoleProfile.USER:
                    res = RoleProfileType.User;
                    break;
                case KalturaUserRoleProfile.PARTNER:
                    res = RoleProfileType.Partner;
                    break;
                case KalturaUserRoleProfile.PERMISSION_EMBEDDED:
                    res = RoleProfileType.PermissionEmbedded;
                    break;
                default:
                    res = RoleProfileType.Profile;
                    break;
            }

            return res;
        }

        private static KalturaUserRoleProfile? ConvertRoleProfileType(RoleProfileType? profile)
        {
            KalturaUserRoleProfile? res = null;

            switch (profile)
            {
                case RoleProfileType.System:
                    res = KalturaUserRoleProfile.SYSTEM;
                    break;
                case RoleProfileType.User:
                    res = KalturaUserRoleProfile.USER;
                    break;
                case RoleProfileType.Partner:
                    res = KalturaUserRoleProfile.PARTNER;
                    break;
                case RoleProfileType.PermissionEmbedded:
                    res = KalturaUserRoleProfile.PERMISSION_EMBEDDED;
                    break;
                default:
                    res = KalturaUserRoleProfile.PROFILE;
                    break;
            }

            return res;
        }

        private static string ToCSV<T>(IEnumerable<T> v) => string.Join(",", v);

        private static KalturaDeviceFamilyType ConvertToKalturaDeviceFamilyType(long deviceFamilyId)
        {
            const long maxSystemId = 49;
            return deviceFamilyId <= maxSystemId
                ? KalturaDeviceFamilyType.System
                : KalturaDeviceFamilyType.Custom;
        }

        private static KalturaDeviceBrandType ConvertToKalturaDeviceBrandType(long deviceBrandId)
        {
            const long maxSystemId = 999;
            return deviceBrandId <= maxSystemId
                ? KalturaDeviceBrandType.System
                : KalturaDeviceBrandType.Custom;
        }

        #endregion

        private static KalturaStringValueArray ConvertListToStringValueArray(List<string> list)
        {
            if (list == null)
            {
                return null;
            }

            KalturaStringValueArray stringValueArray = new KalturaStringValueArray();

            foreach (var stringValue in list)
            {
                stringValueArray.Objects.Add(new KalturaStringValue() { value = stringValue });
            }

            return stringValueArray;
        }

        private static List<string> ConvertStringValueArrayToList(KalturaStringValueArray values)
        {
            if (values == null || values.Objects == null)
            {
                return null;
            }

            List<string> stringList = new List<string>();

            foreach (var stringValue in values.Objects)
            {
                stringList.Add(stringValue.value);
            }

            return stringList;
        }

        private static IDictionary<string, IEnumerable<string>> BuildStringAndStringEnumerableDictionary(SerializableDictionary<string, KalturaStringValueArray> dictionary)
        {
            return dictionary?.ToDictionary(
                x => x.Key,
                x => x.Value.Objects.Select(_ => _.value));
        }

        private static SerializableDictionary<string, KalturaStringValueArray> BuildStringAndKalturaStringValueArraySerializableDictionary(IDictionary<string, IEnumerable<string>> dictionary)
        {
            if (dictionary == null)
            {
                return null;
            }

            var serializableDictionary = new SerializableDictionary<string, KalturaStringValueArray>();
            foreach (var item in dictionary)
            {
                var stringValueArray = BuildKalturaStringValueArray(item.Value);
                serializableDictionary.Add(item.Key, stringValueArray);
            }

            return serializableDictionary;
        }

        public static KalturaStringValueArray BuildKalturaStringValueArray(IEnumerable<string> list)
        {
            if (list == null)
            {
                return null;
            }

            var stringValueArray = new KalturaStringValueArray();
            foreach (var item in list)
            {
                stringValueArray.Objects.Add(new KalturaStringValue { value = item });
            }

            return stringValueArray;
        }
    }
}