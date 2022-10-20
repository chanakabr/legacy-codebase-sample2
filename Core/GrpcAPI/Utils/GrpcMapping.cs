using ApiObjects;
using ApiObjects.Billing;
using AutoMapper;
using AutoMapper.Configuration;
using Core.Pricing;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using phoenix;
using System;
using System.Linq;
using System.Linq.Expressions;
using AssetRule = phoenix.AssetRule;
using SlimAsset = phoenix.SlimAsset;
using Status = phoenix.Status;

namespace GrpcAPI.Utils
{
    public static class GrpcMapping
    {
        public static IMappingExpression<TSource, TDestination> Ignore<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> map,
            Expression<Func<TDestination, object>> selector)
        {
            map.ForMember(selector, config => config.Ignore());
            return map;
        }

        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            cfg.ForAllPropertyMaps(
                map => map.DestinationPropertyType.IsGenericType &&
                       map.DestinationPropertyType.GetGenericTypeDefinition() == typeof(RepeatedField<>),
                (map, options) => options.UseDestinationValue());


            cfg.CreateMap<ApiObjects.MediaMarks.DevicePlayData, DevicePlayData>()
                .ForMember(dest => dest.UDID, opt => opt.MapFrom(src => src.UDID))
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.PlayType, opt => opt.MapFrom(src => src.playType))
                .ForMember(dest => dest.AssetAction, opt => opt.MapFrom(src => src.AssetAction))
                .ForMember(dest => dest.TimeStamp, opt => opt.MapFrom(src => src.TimeStamp))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.MediaConcurrencyRuleIds, opt => opt.MapFrom(src => src.MediaConcurrencyRuleIds))
                .ForMember(dest => dest.AssetMediaConcurrencyRuleIds,
                    opt => opt.MapFrom(src => src.AssetMediaConcurrencyRuleIds))
                .ForMember(dest => dest.AssetEpgConcurrencyRuleIds,
                    opt => opt.MapFrom(src => src.AssetEpgConcurrencyRuleIds))
                .ForMember(dest => dest.DeviceFamilyId, opt => opt.MapFrom(src => src.DeviceFamilyId))
                .ForMember(dest => dest.NpvrId, opt => opt.MapFrom(src => src.NpvrId))
                .ForMember(dest => dest.ProgramId, opt => opt.MapFrom(src => src.ProgramId))
                .ForMember(dest => dest.DomainId, opt => opt.MapFrom(src => src.DomainId))
                .ForMember(dest => dest.PlayCycleKey, opt => opt.MapFrom(src => src.PlayCycleKey))
                .ForMember(dest => dest.BookmarkEventThreshold, opt => opt.MapFrom(src => src.BookmarkEventThreshold))
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductType))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Revoke, opt => opt.MapFrom(src => src.Revoke))
                .ForMember(dest => dest.LinearWatchHistoryThreshold,
                    opt => opt.MapFrom(src => src.LinearWatchHistoryThreshold));

            cfg.CreateMap<DevicePlayData, ApiObjects.MediaMarks.DevicePlayData>()
                .ForMember(dest => dest.UDID, opt => opt.MapFrom(src => src.UDID))
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.playType, opt => opt.MapFrom(src => src.PlayType))
                .ForMember(dest => dest.AssetAction, opt => opt.MapFrom(src => src.AssetAction))
                .ForMember(dest => dest.TimeStamp, opt => opt.MapFrom(src => src.TimeStamp))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.MediaConcurrencyRuleIds, opt => opt.MapFrom(src => src.MediaConcurrencyRuleIds))
                .ForMember(dest => dest.AssetMediaConcurrencyRuleIds,
                    opt => opt.MapFrom(src => src.AssetMediaConcurrencyRuleIds))
                .ForMember(dest => dest.AssetEpgConcurrencyRuleIds,
                    opt => opt.MapFrom(src => src.AssetEpgConcurrencyRuleIds))
                .ForMember(dest => dest.DeviceFamilyId, opt => opt.MapFrom(src => src.DeviceFamilyId))
                .ForMember(dest => dest.NpvrId, opt => opt.MapFrom(src => src.NpvrId))
                .ForMember(dest => dest.ProgramId, opt => opt.MapFrom(src => src.ProgramId))
                .ForMember(dest => dest.DomainId, opt => opt.MapFrom(src => src.DomainId))
                .ForMember(dest => dest.PlayCycleKey, opt => opt.MapFrom(src => src.PlayCycleKey))
                .ForMember(dest => dest.BookmarkEventThreshold, opt => opt.MapFrom(src => src.BookmarkEventThreshold))
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductType))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Revoke, opt => opt.MapFrom(src => src.Revoke))
                .ForMember(dest => dest.LinearWatchHistoryThreshold,
                    opt => opt.MapFrom(src => src.LinearWatchHistoryThreshold));

            cfg.CreateMap<ApiObjects.Response.Status, Status>()
                .ForMember(dest => dest.Args, opt => opt.MapFrom(src => src.Args))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code));

            cfg.CreateMap<Core.ConditionalAccess.MediaFileItemPricesContainer, MediaFileEntitlementContainer>()
                .ForMember(dest => dest.MediaFileID, opt => opt.MapFrom(src => src.m_nMediaFileID))
                .ForMember(dest => dest.EntitlementContainer, opt => opt.MapFrom(src => src.m_oItemPrices))
                .ReverseMap();

            cfg.CreateMap<Core.ConditionalAccess.ItemPriceContainer, EntitlementContainer>()
                .ForMember(dest => dest.SubscriptionId,
                    opt => opt.MapFrom(src => src.m_relevantSub.m_sObjectCode))
                .ForMember(dest => dest.PrePaidId,
                    opt => opt.MapFrom(src => src.m_relevantPP.m_ObjectCode))
                .ForMember(dest => dest.CollectionId,
                    opt => opt.MapFrom(src => src.m_relevantCol.m_sObjectCode))
                .ForMember(dest => dest.PPVId, opt => opt.MapFrom(src => src.m_sPPVModuleCode))
                .ForMember(dest => dest.PriceReason, opt => opt.MapFrom(src => src.m_PriceReason))
                .ForMember(dest => dest.Price,
                    opt => opt.MapFrom(src => src.m_oPrice.m_dPrice))
                .ForMember(dest => dest.Currency,
                    opt => opt.MapFrom(src => src.m_oPrice.m_oCurrency.m_sCurrencyCD3))
                .ForMember(dest => dest.StartDate,
                    opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.m_dtStartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.m_dtEndDate)))
                .ForMember(dest => dest.RelatedMediaFileIDs, opt => opt.MapFrom(src => src.m_lRelatedMediaFileIDs))
                .ForMember(dest => dest.PurchasedMediaFileID, opt => opt.MapFrom(src => src.m_lPurchasedMediaFileID))
                .ForPath(dest => dest.UsageModule,
                    opt =>
                    {
                        opt.MapFrom(src =>
                            src.m_relevantSub != null && src.m_relevantSub.m_oSubscriptionUsageModule != null ? new phoenix.UsageModule()
                            {
                                IsOfflinePlayback = src.m_relevantSub.m_oSubscriptionUsageModule.m_bIsOfflinePlayBack,
                                ViewLifeCycle = src.m_relevantSub.m_oSubscriptionUsageModule.m_tsViewLifeCycle,
                                MaxUsageLifeCycle = src.m_relevantSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle
                            }
                            : src.m_relevantCol != null && src.m_relevantCol.m_oCollectionUsageModule != null ? new phoenix.UsageModule()
                            {
                                IsOfflinePlayback = src.m_relevantCol.m_oCollectionUsageModule.m_bIsOfflinePlayBack,
                                ViewLifeCycle = src.m_relevantCol.m_oCollectionUsageModule.m_tsViewLifeCycle,
                                MaxUsageLifeCycle = src.m_relevantCol.m_oCollectionUsageModule.m_tsMaxUsageModuleLifeCycle
                            } : null);
                    })
                .ReverseMap()
                .ForPath(src => src.m_dtStartDate,
                    opt => opt.MapFrom(dest => FromUtcUnixTimestampSeconds(dest.StartDate)))
                .ForPath(src => src.m_dtEndDate, opt => opt.MapFrom(dest => FromUtcUnixTimestampSeconds(dest.EndDate)))
                .ForPath(src => src.m_relevantSub,
                    opt =>
                    {
                        opt.MapFrom(dest =>
                            string.IsNullOrEmpty(dest.SubscriptionId)
                                ? null
                                : new Core.Pricing.Subscription
                                {
                                    m_sObjectCode = dest.SubscriptionId,
                                    m_SubscriptionCode = dest.SubscriptionId,
                                    m_oSubscriptionUsageModule = dest.UsageModule == null ? null : new Core.Pricing.UsageModule()
                                    {
                                        m_bIsOfflinePlayBack = dest.UsageModule.IsOfflinePlayback,
                                        m_tsViewLifeCycle = dest.UsageModule.ViewLifeCycle,
                                        m_tsMaxUsageModuleLifeCycle = dest.UsageModule.MaxUsageLifeCycle
                                    }
                                });
                    })
                .ForPath(src => src.m_relevantCol,
                    opt =>
                    {
                        opt.MapFrom(dest =>
                            string.IsNullOrEmpty(dest.CollectionId)
                                ? null
                                : new Core.Pricing.Collection
                                {
                                    m_sObjectCode = dest.CollectionId,
                                    m_CollectionCode = dest.CollectionId,
                                    m_oCollectionUsageModule = dest.UsageModule == null ? null : new Core.Pricing.UsageModule()
                                    {
                                        m_bIsOfflinePlayBack = dest.UsageModule.IsOfflinePlayback,
                                        m_tsViewLifeCycle = dest.UsageModule.ViewLifeCycle,
                                        m_tsMaxUsageModuleLifeCycle = dest.UsageModule.MaxUsageLifeCycle
                                    }
                                });
                    })
                .ForPath(src => src.m_relevantPP, opt =>
                {
                    opt.MapFrom(dest => dest.PrePaidId == 0
                        ? null
                        : new PrePaidModule
                        {
                            m_ObjectCode = dest.PrePaidId
                        });
                })
                .ForPath(src => src.m_oPrice,
                    opt =>
                    {
                        opt.MapFrom(dest => new Core.Pricing.Price
                                {
                                    m_dPrice = dest.Price,
                                    m_oCurrency =  new Core.Pricing.Currency
                                        {
                                            m_sCurrencyCD3 = dest.Currency
                                        }
                                });
                    });

            cfg.CreateMap<ApiObjects.Rules.SlimAsset, SlimAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));

            cfg.CreateMap<SlimAsset, ApiObjects.Rules.SlimAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));

            cfg.CreateMap<ApiObjects.Rules.AssetRule, AssetRule>()
                .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => src.Actions))
                .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
                .ForPath(dest => dest.Rule.Id, opt => opt.MapFrom(src => src.Id))
                .ForPath(dest => dest.Rule.Description, opt => opt.MapFrom(src => src.Description))
                .ForPath(dest => dest.Rule.Label, opt => opt.MapFrom(src => src.Label))
                .ForPath(dest => dest.Rule.Name, opt => opt.MapFrom(src => src.Name))
                .ForPath(dest => dest.Rule.Status, opt => opt.MapFrom(src => src.Status))
                .ForPath(dest => dest.Rule.GroupId, opt => opt.MapFrom(src => src.GroupId));

            cfg.CreateMap<AssetRule, ApiObjects.Rules.AssetRule>()
                .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => src.Actions))
                .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Rule.Id))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Rule.Description))
                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Rule.Label))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Rule.Name))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Rule.Status))
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.Rule.GroupId));

            cfg.CreateMap<ApiObjects.Rules.RuleCondition, RuleConditionKind>()
                .ConstructUsing(condition =>
                {
                    var conditionKind = new RuleConditionKind();
                    switch (condition.GetType().FullName)
                    {
                        case "ApiObjects.Rules.OrCondition":
                            conditionKind.OrCondition = Mapper.Map<phoenix.OrCondition>(condition);
                            break;
                        case "ApiObjects.Rules.NotRuleCondition":
                            conditionKind.NotRuleCondition = Mapper.Map<phoenix.NotRuleCondition>(condition);
                            break;
                        case "ApiObjects.Rules.BusinessModuleCondition":
                            conditionKind.BusinessModuleCondition =
                                Mapper.Map<phoenix.BusinessModuleCondition>(condition);
                            break;
                        case "ApiObjects.Rules.AssetCondition":
                            conditionKind.AssetCondition = Mapper.Map<phoenix.AssetCondition>(condition);
                            break;
                        case "ApiObjects.Rules.DateCondition":
                            conditionKind.DateCondition = Mapper.Map<phoenix.DateCondition>(condition);
                            break;
                        case "ApiObjects.Rules.ConcurrencyCondition":
                            conditionKind.ConcurrencyCondition =
                                Mapper.Map<phoenix.ConcurrencyCondition>(condition);
                            break;
                        case "ApiObjects.Rules.CountryCondition":
                            conditionKind.CountryCondition = Mapper.Map<phoenix.CountryCondition>(condition);
                            break;
                        case "ApiObjects.Rules.IpRangeCondition":
                            conditionKind.IpRangeCondition = Mapper.Map<phoenix.IpRangeCondition>(condition);
                            break;
                        case "ApiObjects.Rules.HeaderCondition":
                            conditionKind.HeaderCondition = Mapper.Map<phoenix.HeaderCondition>(condition);
                            break;
                        case "ApiObjects.Rules.UserSubscriptionCondition":
                            conditionKind.UserSubscriptionCondition =
                                Mapper.Map<phoenix.UserSubscriptionCondition>(condition);
                            break;
                        case "ApiObjects.Rules.AssetSubscriptionCondition":
                            conditionKind.AssetSubscriptionCondition =
                                Mapper.Map<phoenix.AssetSubscriptionCondition>(condition);
                            break;
                        case "ApiObjects.Rules.UserRoleCondition":
                            conditionKind.UserRoleCondition =
                                Mapper.Map<phoenix.UserRoleCondition>(condition);
                            break;
                        case "ApiObjects.Rules.UdidDynamicListCondition":
                            conditionKind.UdidDynamicListCondition =
                                Mapper.Map<phoenix.UdidDynamicListCondition>(condition);
                            break;
                        case "ApiObjects.Rules.UserSessionProfileCondition":
                            conditionKind.UserSessionProfileCondition =
                                Mapper.Map<phoenix.UserSessionProfileCondition>(condition);
                            break;
                        default:
                            break;
                    }

                    return conditionKind;
                });

            cfg.CreateMap<ApiObjects.Rules.RuleAction, RuleActionKind>()
                .ConstructUsing(action =>
                {
                    var actionKind = new RuleActionKind();
                    switch (action.GetType().FullName)
                    {
                        case "ApiObjects.Rules.AllowPlaybackAction":
                            actionKind.AllowPlaybackAction =
                                Mapper.Map<phoenix.RuleAction.Types.AllowPlaybackAction>(action);
                            break;
                        case "ApiObjects.Rules.AssetBlockAction":
                            actionKind.AssetBlockAction = Mapper.Map<phoenix.RuleAction.Types.AssetBlockAction>(action);
                            break;
                        case "ApiObjects.Rules.BlockPlaybackAction":
                            actionKind.BlockPlaybackAction =
                                Mapper.Map<phoenix.RuleAction.Types.BlockPlaybackAction>(action);
                            break;
                        case "ApiObjects.Rules.ApplyFreePlaybackAction":
                            actionKind.ApplyFreePlaybackAction =
                                Mapper.Map<phoenix.RuleAction.Types.ApplyFreePlaybackAction>(action);
                            break;
                        case "ApiObjects.Rules.ApplyDiscountModuleRuleAction":
                            actionKind.ApplyDiscountModuleRuleAction =
                                Mapper.Map<phoenix.RuleAction.Types.ApplyDiscountModuleRuleAction>(action);
                            break;
                        case "ApiObjects.Rules.ApplyPlaybackAdapterRuleAction":
                            actionKind.ApplyPlaybackAdapterRuleAction =
                                Mapper.Map<phoenix.RuleAction.Types.ApplyPlaybackAdapterRuleAction>(action);
                            break;
                        case "ApiObjects.Rules.AssetUserRuleBlockAction":
                            actionKind.AssetUserRuleBlockAction =
                                Mapper.Map<phoenix.RuleAction.Types.AssetUserRuleBlockAction>(action);
                            break;
                        case "ApiObjects.Rules.AssetUserRuleFilterAction":
                            actionKind.AssetUserRuleFilterAction =
                                Mapper.Map<phoenix.RuleAction.Types.AssetUserRuleFilterAction>(action);
                            break;
                        case "ApiObjects.Rules.EndDateOffsetRuleAction":
                            actionKind.EndDateOffsetRuleAction =
                                Mapper.Map<phoenix.RuleAction.Types.EndDateOffsetRuleAction>(action);
                            break;
                        case "ApiObjects.Rules.StartDateOffsetRuleAction":
                            actionKind.StartDateOffsetRuleAction =
                                Mapper.Map<phoenix.RuleAction.Types.StartDateOffsetRuleAction>(action);
                            break;
                        case "ApiObjects.Rules.AssetLifeCycleTagTransitionAction":
                            actionKind.AssetLifeCycleTagTransitionAction =
                                Mapper.Map<phoenix.RuleAction.Types.AssetLifeCycleTagTransitionAction>(action);
                            break;
                        case "ApiObjects.Rules.AssetLifeCycleBuisnessModuleTransitionAction":
                            actionKind.AssetLifeCycleBuisnessModuleTransitionAction =
                                Mapper.Map<phoenix.RuleAction.Types.AssetLifeCycleBuisnessModuleTransitionAction>(
                                    action);
                            break;
                        default:
                            break;
                    }

                    return actionKind;
                });

            cfg.CreateMap<phoenix.OrCondition, ApiObjects.Rules.OrCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Not, opt => opt.MapFrom(src => src.Not))
                .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
                .ReverseMap();

            cfg.CreateMap<phoenix.BusinessModuleCondition, ApiObjects.Rules.BusinessModuleCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.BusinessModuleId, opt => opt.MapFrom(src => src.BusinessModuleId))
                .ForMember(dest => dest.BusinessModuleType, opt => opt.MapFrom(src => src.BusinessModuleType))
                .ReverseMap();

            cfg.CreateMap<phoenix.AssetCondition, ApiObjects.Rules.AssetCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Ksql, opt => opt.MapFrom(src => src.Ksql))
                .ReverseMap();

            cfg.CreateMap<phoenix.ConcurrencyCondition, ApiObjects.Rules.ConcurrencyCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Limit, opt => opt.MapFrom(src => src.Limit))
                .ForMember(dest => dest.RestrictionPolicy, opt => opt.MapFrom(src => src.RestrictionPolicy))
                .ReverseMap();

            cfg.CreateMap<phoenix.DateCondition, ApiObjects.Rules.DateCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ReverseMap();

            cfg.CreateMap<phoenix.IpRangeCondition, ApiObjects.Rules.IpRangeCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.FromIp, opt => opt.MapFrom(src => src.FromIp))
                .ForMember(dest => dest.ToIp, opt => opt.MapFrom(src => src.ToIp))
                .ForMember(dest => dest.IpFrom, opt => opt.MapFrom(src => src.IpFrom))
                .ForMember(dest => dest.IpTo, opt => opt.MapFrom(src => src.IpTo))
                .ReverseMap();

            cfg.CreateMap<phoenix.CountryCondition, ApiObjects.Rules.CountryCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Not, opt => opt.MapFrom(src => src.Not))
                .ForMember(dest => dest.Countries, opt => opt.MapFrom(src => src.Countries))
                .ReverseMap();

            cfg.CreateMap<phoenix.HeaderCondition, ApiObjects.Rules.HeaderCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Not, opt => opt.MapFrom(src => src.Not))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                .ReverseMap();

            cfg.CreateMap<phoenix.UserSubscriptionCondition, ApiObjects.Rules.UserSubscriptionCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();

            cfg.CreateMap<phoenix.AssetSubscriptionCondition, ApiObjects.Rules.AssetSubscriptionCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();

            cfg.CreateMap<phoenix.UdidDynamicListCondition, ApiObjects.Rules.UdidDynamicListCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            cfg.CreateMap<phoenix.UserRoleCondition, ApiObjects.Rules.UserRoleCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.RoleIds, opt => opt.MapFrom(src => src.RoleIds))
                .ReverseMap();

            cfg.CreateMap<phoenix.UserSessionProfileCondition, ApiObjects.Rules.UserSessionProfileCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            cfg.CreateMap<phoenix.SegmentsCondition, ApiObjects.Rules.SegmentsCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.SegmentIds, opt => opt.MapFrom(src => src.SegmentIds))
                .ReverseMap();

            cfg.CreateMap<phoenix.DeviceBrandCondition, ApiObjects.Rules.DeviceBrandCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.IdIn, opt => opt.MapFrom(src => src.IdIn))
                .ReverseMap();

            cfg.CreateMap<phoenix.DeviceFamilyCondition, ApiObjects.Rules.DeviceFamilyCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.IdIn, opt => opt.MapFrom(src => src.IdIn))
                .ReverseMap();

            cfg.CreateMap<phoenix.DeviceManufacturerCondition, ApiObjects.Rules.DeviceManufacturerCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.IdIn, opt => opt.MapFrom(src => src.IdIn))
                .ReverseMap();

            cfg.CreateMap<phoenix.DeviceModelCondition, ApiObjects.Rules.DeviceModelCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.RegexEqual, opt => opt.MapFrom(src => src.RegexEqual))
                .ReverseMap();

            cfg.CreateMap<phoenix.DynamicKeysCondition, ApiObjects.Rules.DynamicKeysCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.Values, opt => opt.MapFrom(src => src.Values))
                .ReverseMap();

            cfg.CreateMap<phoenix.DeviceDynamicDataCondition, ApiObjects.Rules.DeviceDynamicDataCondition>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                .ReverseMap();

            cfg.CreateMap<phoenix.RuleAction, ApiObjects.Rules.RuleAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();

            cfg.CreateMap<phoenix.RuleAction.Types.AssetBlockAction, ApiObjects.Rules.AssetBlockAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();

            cfg.CreateMap<phoenix.RuleAction.Types.AssetUserRuleBlockAction,
                    ApiObjects.Rules.AssetUserRuleBlockAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();

            cfg.CreateMap<phoenix.RuleAction.Types.AllowPlaybackAction, ApiObjects.Rules.AllowPlaybackAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();

            cfg.CreateMap<phoenix.RuleAction.Types.BlockPlaybackAction, ApiObjects.Rules.BlockPlaybackAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();

            cfg.CreateMap<phoenix.RuleAction.Types.TimeOffsetRuleAction, ApiObjects.Rules.TimeOffsetRuleAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Offset, opt => opt.MapFrom(src => src.Offset))
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone))
                .ReverseMap();

            cfg.CreateMap<phoenix.RuleAction.Types.TimeOffsetRuleAction, ApiObjects.Rules.EndDateOffsetRuleAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Offset, opt => opt.MapFrom(src => src.Offset))
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone))
                .ReverseMap();

            cfg.CreateMap<phoenix.RuleAction.Types.TimeOffsetRuleAction, ApiObjects.Rules.StartDateOffsetRuleAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Offset, opt => opt.MapFrom(src => src.Offset))
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone))
                .ReverseMap();

            cfg.CreateMap<phoenix.RuleAction.Types.AssetUserRuleFilterAction,
                    ApiObjects.Rules.AssetUserRuleFilterAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.ApplyOnChannel, opt => opt.MapFrom(src => src.ApplyOnChannel))
                .ReverseMap();

            cfg.CreateMap<phoenix.RuleAction.Types.ApplyDiscountModuleRuleAction,
                    ApiObjects.Rules.ApplyDiscountModuleRuleAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.DiscountModuleId, opt => opt.MapFrom(src => src.DiscountModuleId))
                .ReverseMap();

            cfg.CreateMap<phoenix.RuleAction.Types.AssetLifeCycleTransitionAction,
                    ApiObjects.Rules.AssetLifeCycleTransitionAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => src.ActionType))
                .ForMember(dest => dest.TransitionType, opt => opt.MapFrom(src => src.TransitionType))
                .ReverseMap();

            cfg.CreateMap<phoenix.RuleAction.Types.AssetLifeCycleTagTransitionAction,
                    ApiObjects.Rules.AssetLifeCycleTagTransitionAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => src.ActionType))
                .ForMember(dest => dest.TransitionType, opt => opt.MapFrom(src => src.TransitionType))
                .ForMember(dest => dest.TagIds, opt => opt.MapFrom(src => src.TagIds))
                .ReverseMap();

            cfg.CreateMap<phoenix.RuleAction.Types.LifeCycleFileTypesAndPpvsTransitions,
                    ApiObjects.AssetLifeCycleRules.LifeCycleFileTypesAndPpvsTransitions>()
                .ForMember(dest => dest.FileTypeIds, opt => opt.MapFrom(src => src.FileTypeIds))
                .ForMember(dest => dest.PpvIds, opt => opt.MapFrom(src => src.PpvIds))
                .ReverseMap();

            cfg.CreateMap<phoenix.RuleAction.Types.AssetLifeCycleBuisnessModuleTransitionAction,
                    ApiObjects.Rules.AssetLifeCycleBuisnessModuleTransitionAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => src.ActionType))
                .ForMember(dest => dest.TransitionType, opt => opt.MapFrom(src => src.TransitionType))
                .ForMember(dest => dest.Transitions, opt => opt.MapFrom(src => src.Transitions))
                .ReverseMap();

            cfg.CreateMap<phoenix.RuleAction.Types.ApplyFreePlaybackAction, ApiObjects.Rules.ApplyFreePlaybackAction>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();

            cfg.CreateMap<Core.Users.DeviceContainer, deviceFamilyData>()
                .ForMember(dest => dest.FamilyId, opt => opt.MapFrom(src => src.m_deviceFamilyID))
                .ForMember(dest => dest.Concurrency, opt => opt.MapFrom(src => src.m_oLimitationsManager.Concurrency))
                .ForMember(dest => dest.Udids,
                    opt => opt.MapFrom(src => src.DeviceInstances.Select(d => d.m_deviceUDID)));

            cfg.CreateMap<ApiObjects.TimeShiftedTv.Recording, Recording>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.EpgId))
                .ForMember(dest => dest.ChannelId, opt => opt.MapFrom(src => src.ChannelId))
                .ForMember(dest => dest.RecordingStatus, opt => opt.MapFrom(src => src.RecordingStatus))
                .ForMember(dest => dest.ExternalRecordingId, opt => opt.MapFrom(src => src.ExternalRecordingId))
                .ForMember(dest => dest.EpgStartDate,
                    opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.EpgStartDate)))
                .ForMember(dest => dest.EpgEndDate,
                    opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.EpgEndDate)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.GetStatusRetries, opt => opt.MapFrom(src => src.GetStatusRetries))
                .ForMember(dest => dest.ProtectedUntilDate, opt => opt.MapFrom(src => src.ProtectedUntilDate))
                .ForMember(dest => dest.ViewableUntilDate, opt => opt.MapFrom(src => src.ViewableUntilDate))
                .ForMember(dest => dest.CreateDate,
                    opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.CreateDate)))
                .ForMember(dest => dest.UpdateDate,
                    opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.UpdateDate)))
                .ForMember(dest => dest.Crid, opt => opt.MapFrom(src => src.Crid))
                .ForMember(dest => dest.IsExternalRecording, opt => opt.MapFrom(src => src.isExternalRecording))
                .ForMember(dest => dest.IsProtected, opt => opt.MapFrom(src => src.IsProtected))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

            cfg.CreateMap<Core.Catalog.Response.MediaObj, GetMediaByIdResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_sDescription))
                .ForMember(dest => dest.ExternalIDs, opt => opt.MapFrom(src => src.m_ExternalIDs))
                .ForMember(dest => dest.LastWatchedDevice, opt => opt.MapFrom(src => src.m_sLastWatchedDevice))
                .ForMember(dest => dest.SiteUserGuid, opt => opt.MapFrom(src => src.m_sSiteUserGuid))
                .ForMember(dest => dest.EntryId, opt => opt.MapFrom(src => src.EntryId))
                .ForMember(dest => dest.CoGuid, opt => opt.MapFrom(src => src.CoGuid))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.EnableCDVR, opt => opt.MapFrom(src => src.EnableCDVR))
                .ForMember(dest => dest.EnableCatchUp, opt => opt.MapFrom(src => src.EnableCatchUp))
                .ForMember(dest => dest.EnableStartOver, opt => opt.MapFrom(src => src.EnableStartOver))
                .ForMember(dest => dest.EnableTrickPlay, opt => opt.MapFrom(src => src.EnableTrickPlay))
                .ForMember(dest => dest.CatchUpBuffer, opt => opt.MapFrom(src => src.CatchUpBuffer))
                .ForMember(dest => dest.TrickPlayBuffer, opt => opt.MapFrom(src => src.TrickPlayBuffer))
                .ForMember(dest => dest.EnableRecordingPlaybackNonEntitledChannel,
                    opt => opt.MapFrom(src => src.EnableRecordingPlaybackNonEntitledChannel))
                .ForMember(dest => dest.ExternalCdvrId, opt => opt.MapFrom(src => src.ExternalCdvrId))
                .ForMember(dest => dest.WatchPermissionRule, opt => opt.MapFrom(src => src.WatchPermissionRule))
                .ForMember(dest => dest.GeoblockRule, opt => opt.MapFrom(src => src.GeoblockRule))
                .ForMember(dest => dest.DeviceRule, opt => opt.MapFrom(src => src.DeviceRule));

            cfg.CreateMap<Scheduling, GetProgramScheduleResponse>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.StartDate)))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => ToUtcUnixTimestampSeconds(src.EndTime)));

            cfg.CreateMap<MeidaMaper, MediaMapper>()
                .ForMember(dest => dest.MediaFileID, opt => opt.MapFrom(src => src.m_nMediaFileID))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.m_sProductCode))
                .ForMember(dest => dest.MediaID, opt => opt.MapFrom(src => src.m_nMediaID));

            cfg.CreateMap<phoenix.MediaFile, ApiObjects.MediaFile>()
                .ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => src.MediaId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.TypeId))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                .ForMember(dest => dest.AltUrl, opt => opt.MapFrom(src => src.AltUrl))
                .ForMember(dest => dest.DirectUrl, opt => opt.MapFrom(src => src.DirectUrl))
                .ForMember(dest => dest.AltDirectUrl, opt => opt.MapFrom(src => src.AltDirectUrl))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId))
                .ForMember(dest => dest.StreamerType, opt => opt.MapFrom(src => src.StreamerType))
                .ForMember(dest => dest.IsTrailer, opt => opt.MapFrom(src => src.IsTrailer))
                .ForMember(dest => dest.CdnId, opt => opt.MapFrom(src => src.CdnId))
                .ForMember(dest => dest.AltCdnId, opt => opt.MapFrom(src => src.AltCdnId))
                .ForMember(dest => dest.DrmId, opt => opt.MapFrom(src => src.DrmId))
                .ForMember(dest => dest.AdsPolicy, opt => opt.MapFrom(src => src.AdsPolicy))
                .ForMember(dest => dest.AdsParam, opt => opt.MapFrom(src => src.AdsParam))
                .ForMember(dest => dest.Opl, opt => opt.MapFrom(src => src.Opl))
                .ForMember(dest => dest.BusinessModuleDetails, opt => opt.MapFrom(src => src.BusinessModuleDetails))
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.GroupId))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.Labels)).ReverseMap();

            cfg.CreateMap<PaymentGateway, PaymentGatewayProfile>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
                .ForMember(dest => dest.PaymentMethods, opt => opt.MapFrom(src => src.PaymentMethods))
                .ForMember(dest => dest.SupportPaymentMethod, opt => opt.MapFrom(src => src.SupportPaymentMethod))
                .ForMember(dest => dest.PendingInterval, opt => opt.MapFrom(src => src.PendingInterval))
                .ForMember(dest => dest.PendingRetries, opt => opt.MapFrom(src => src.PendingRetries))
                .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret))
                .ForMember(dest => dest.ExternalVerification, opt => opt.MapFrom(src => src.ExternalVerification))
                .ForMember(dest => dest.IsAsyncPolicy, opt => opt.MapFrom(src => src.IsAsyncPolicy))
                .ForMember(dest => dest.TransactUrl, opt => opt.MapFrom(src => src.TransactUrl))
                .ForMember(dest => dest.StatusUrl, opt => opt.MapFrom(src => src.StatusUrl))
                .ForMember(dest => dest.RenewUrl, opt => opt.MapFrom(src => src.RenewUrl))
                .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier))
                .ForMember(dest => dest.SkipSettings, opt => opt.MapFrom(src => src.SkipSettings))
                .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => src.Settings))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Selected, opt => opt.MapFrom(src => src.Selected))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
        }

        private static Timestamp ToUtcUnixTimestampSeconds(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return Timestamp.FromDateTime(DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc));
            }

            return null;
        }

        private static DateTime? FromUtcUnixTimestampSeconds(Timestamp? timestamp)
        {
            if (timestamp != null)
            {
                return timestamp.ToDateTime();
            }

            return null;
        }
    }
}