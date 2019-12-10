using ApiObjects;
using ApiObjects.Segmentation;
using AutoMapper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Models.Segmentation;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class SegmentationMapings
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            #region Segmentation Type

            // Segmentation type
            cfg.CreateMap<KalturaSegmentationType, SegmentationType>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
                .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => src.Actions))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.Version, opt => opt.Ignore());

            cfg.CreateMap<SegmentationType, KalturaSegmentationType>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.Conditions))
                .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => src.Actions))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version));

            // Segmentation source
            cfg.CreateMap<KalturaSegmentSource, SegmentSource>()
                .Include<KalturaMonetizationSource, MonetizationSource>()
                .Include<KalturaContentSource, ContentSource>()
                .Include<KalturaUserDynamicDataSource, UserDynamicDataSource>()
                ;

            cfg.CreateMap<SegmentSource, KalturaSegmentSource>()
                .Include<MonetizationSource, KalturaMonetizationSource>()
                .Include<ContentSource, KalturaContentSource>()
                .Include<UserDynamicDataSource, KalturaUserDynamicDataSource>()
                ;

            // Monetization source
            cfg.CreateMap<KalturaMonetizationSource, MonetizationSource>()
                .ForMember(dest => dest.Operator, opt => opt.ResolveUsing(src => ConvertMathematicalOperator(src.Operator)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertMonetizationType(src.Type)))
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days))
                ;

            cfg.CreateMap<MonetizationSource, KalturaMonetizationSource>()
                .ForMember(dest => dest.Operator, opt => opt.ResolveUsing(src => ConvertMathematicalOperator(src.Operator)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertMonetizationType(src.Type)))
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days))
                ;

            // Content source
            cfg.CreateMap<KalturaContentSource, ContentSource>()
                .ForMember(dest => dest.Field, opt => opt.MapFrom(src => src.Field))
                ;

            cfg.CreateMap<ContentSource, KalturaContentSource>()
                .ForMember(dest => dest.Field, opt => opt.MapFrom(src => src.Field))
                ;

            // User dnymaic data source
            cfg.CreateMap<KalturaUserDynamicDataSource, UserDynamicDataSource>()
                .ForMember(d => d.Field, opt => opt.MapFrom(s => s.Field))
                ;

            cfg.CreateMap<UserDynamicDataSource, KalturaUserDynamicDataSource>()
                .ForMember(d => d.Field, opt => opt.MapFrom(s => s.Field))
                ;

            // segmentation action
            cfg.CreateMap<KalturaBaseSegmentAction, SegmentAction>()
                .Include<KalturaAssetOrderSegmentAction, SegmentAssetOrderAction>()
                .Include<KalturaBlockPlaybackSegmentAction, SegmentBlockPlaybackAction>()
                ;

            cfg.CreateMap<SegmentAction, KalturaBaseSegmentAction>()
                .Include<SegmentAssetOrderAction, KalturaAssetOrderSegmentAction>()
                .Include<SegmentBlockPlaybackAction, KalturaBlockPlaybackSegmentAction>()
                ;

            // segment order action
            cfg.CreateMap<KalturaAssetOrderSegmentAction, SegmentAssetOrderAction>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Values, opt => opt.MapFrom(src => src.Values.Select(x => x.value).ToList()))
                ;

            cfg.CreateMap<SegmentAssetOrderAction, KalturaAssetOrderSegmentAction>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Values, opt => opt.MapFrom(src => src.Values.Select(x => new KalturaStringValue(null) { value = x }).ToList()))
                ;

            // segment playback block action
            cfg.CreateMap<KalturaBlockPlaybackSegmentAction, SegmentBlockPlaybackAction>()
                .ForMember(dest => dest.Ksql, opt => opt.MapFrom(src => src.KSQL))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertFromSegmentBlockPlaybackActionType(src.Type)))
                ;

            cfg.CreateMap<SegmentBlockPlaybackAction, KalturaBlockPlaybackSegmentAction>()
                .ForMember(dest => dest.KSQL, opt => opt.MapFrom(src => src.Ksql))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertToSegmentBlockPlaybackActionType(src.Type)))
                ;

            // segmentation condition
            cfg.CreateMap<KalturaBaseSegmentCondition, SegmentCondition>()
                .Include<KalturaUserDataCondition, SegmentUserDataCondition>()
                .Include<KalturaContentScoreCondition, ContentScoreCondition>()
                .Include<KalturaMonetizationCondition, MonetizationCondition>()
                ;

            cfg.CreateMap<SegmentCondition, KalturaBaseSegmentCondition>()
                .Include<SegmentUserDataCondition, KalturaUserDataCondition>()
                .Include<ContentScoreCondition, KalturaContentScoreCondition>()
                .Include<MonetizationCondition, KalturaMonetizationCondition>()
                ;

            // user data condition
            cfg.CreateMap<KalturaUserDataCondition, SegmentUserDataCondition>()
                .ForMember(dest => dest.Field, opt => opt.MapFrom(src => src.Field))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                ;

            cfg.CreateMap<SegmentUserDataCondition, KalturaUserDataCondition>()
                .ForMember(dest => dest.Field, opt => opt.MapFrom(src => src.Field))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                ;

            // content score condition
            cfg.CreateMap<KalturaContentScoreCondition, ContentScoreCondition>()
                .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => src.Actions))
                .ForMember(dest => dest.MinScore, opt => opt.MapFrom(src => src.MinScore))
                .ForMember(dest => dest.MaxScore, opt => opt.MapFrom(src => src.MaxScore))
                .ForMember(dest => dest.Field, opt => opt.MapFrom(src => src.Field))
                .ForMember(dest => dest.Values, opt => opt.MapFrom(src => src.Values.Select(x => x.value).ToList()))
                ;

            cfg.CreateMap<ContentScoreCondition, KalturaContentScoreCondition>()
                .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => src.Actions))
                .ForMember(dest => dest.MinScore, opt => opt.MapFrom(src => src.MinScore))
                .ForMember(dest => dest.MaxScore, opt => opt.MapFrom(src => src.MaxScore))
                .ForMember(dest => dest.Field, opt => opt.MapFrom(src => src.Field))
                .ForMember(dest => dest.Values, opt => opt.MapFrom(src => src.Values.Select(x => new KalturaStringValue(null) { value = x }).ToList()))
                ;

            // content action condition
            cfg.CreateMap<KalturaContentActionCondition, ContentActionCondition>()
                .ForMember(dest => dest.Action, opt => opt.MapFrom(src => ConvertContentAction(src.Action)))
                .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Length))
                .ForMember(dest => dest.LengthType, opt => opt.MapFrom(src => ConvertLengthType(src.LengthType)))
                .ForMember(dest => dest.Multiplier, opt => opt.MapFrom(src => src.Multiplier))
                ;

            cfg.CreateMap<ContentActionCondition, KalturaContentActionCondition>()
                .ForMember(dest => dest.Action, opt => opt.MapFrom(src => ConvertContentAction(src.Action)))
                .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Length))
                .ForMember(dest => dest.LengthType, opt => opt.MapFrom(src => ConvertLengthType(src.LengthType)))
                .ForMember(dest => dest.Multiplier, opt => opt.MapFrom(src => src.Multiplier))
                ;

            //  monetization condition
            cfg.CreateMap<KalturaMonetizationCondition, MonetizationCondition>()
                .ForMember(dest => dest.MinValue, opt => opt.MapFrom(src => src.MinValue))
                .ForMember(dest => dest.MaxValue, opt => opt.MapFrom(src => src.MaxValue))
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertMonetizationType(src.Type)))
                .ForMember(dest => dest.Operator, opt => opt.MapFrom(src => ConvertMathematicalOperator(src.Operator)))
                .ForMember(dest => dest.BusinessModuleIds, opt => opt.MapFrom(src => src.GetBusinessModuleIdIn()))
                ;

            cfg.CreateMap<MonetizationCondition, KalturaMonetizationCondition>()
                .ForMember(dest => dest.MinValue, opt => opt.MapFrom(src => src.MinValue))
                .ForMember(dest => dest.MaxValue, opt => opt.MapFrom(src => src.MaxValue))
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertMonetizationType(src.Type)))
                .ForMember(dest => dest.Operator, opt => opt.MapFrom(src => ConvertMathematicalOperator(src.Operator)))
                .ForMember(dest => dest.BusinessModuleIdIn, opt => opt.MapFrom(src => string.Join(",", src.BusinessModuleIds)))
                ;

            // base segment value
            cfg.CreateMap<KalturaBaseSegmentValue, SegmentBaseValue>()
                .Include<KalturaSegmentValues, SegmentValues>()
                .Include<KalturaSegmentAllValues, SegmentAllValues>()
                .Include<KalturaSegmentRanges, SegmentRanges>()
                .Include<KalturaSingleSegmentValue, SegmentDummyValue>()
                ;

            cfg.CreateMap<SegmentBaseValue, KalturaBaseSegmentValue>()
                .Include<SegmentValues, KalturaSegmentValues>()
                .Include<SegmentAllValues, KalturaSegmentAllValues>()
                .Include<SegmentRanges, KalturaSegmentRanges>()
                .Include<SegmentDummyValue, KalturaSingleSegmentValue>()
                ;

            // segment dummy value
            cfg.CreateMap<SegmentDummyValue, KalturaSingleSegmentValue>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AffectedUsers, opt => opt.MapFrom(src => src.AffectedUsersTtl >= DateTime.UtcNow ? src.AffectedUsers : 0))
                ;

            cfg.CreateMap<KalturaSingleSegmentValue, SegmentDummyValue>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                ;

            // segment value
            cfg.CreateMap<KalturaSegmentValue, SegmentValue>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SystematicName, opt => opt.MapFrom(src => src.SystematicName))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                ;

            cfg.CreateMap<SegmentValue, KalturaSegmentValue>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SystematicName, opt => opt.MapFrom(src => src.SystematicName))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                ;

            // segment values
            cfg.CreateMap<KalturaSegmentValues, SegmentValues>()
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source))
                .ForMember(dest => dest.Values, opt => opt.MapFrom(src => src.Values))
                ;

            cfg.CreateMap<SegmentValues, KalturaSegmentValues>()
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source))
                .ForMember(dest => dest.Values, opt => opt.MapFrom(src => src.Values))
                ;

            // segment all values
            cfg.CreateMap<KalturaSegmentAllValues, SegmentAllValues>()
                .ForMember(dest => dest.NameFormat, opt => opt.MapFrom(src => src.NameFormat))
                ;

            cfg.CreateMap<SegmentAllValues, KalturaSegmentAllValues>()
                .ForMember(dest => dest.NameFormat, opt => opt.MapFrom(src => src.NameFormat))
                ;

            // segment ranges
            cfg.CreateMap<KalturaSegmentRanges, SegmentRanges>()
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source))
                .ForMember(dest => dest.Ranges, opt => opt.MapFrom(src => src.Ranges))
                ;

            cfg.CreateMap<SegmentRanges, KalturaSegmentRanges>()
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source))
                .ForMember(dest => dest.Ranges, opt => opt.MapFrom(src => src.Ranges))
                ;

            // segment range
            cfg.CreateMap<KalturaSegmentRange, SegmentRange>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SystematicName, opt => opt.MapFrom(src => src.SystematicName))
                .ForMember(dest => dest.Equals, opt => opt.MapFrom(src => src.Equals))
                .ForMember(dest => dest.GreaterThan, opt => opt.MapFrom(src => src.GreaterThan))
                .ForMember(dest => dest.GreaterThanOrEquals, opt => opt.MapFrom(src => src.GreaterThanOrEquals))
                .ForMember(dest => dest.LessThan, opt => opt.MapFrom(src => src.LessThan))
                .ForMember(dest => dest.LessThanOrEquals, opt => opt.MapFrom(src => src.LessThanOrEquals))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                ;

            cfg.CreateMap<SegmentRange, KalturaSegmentRange>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SystematicName, opt => opt.MapFrom(src => src.SystematicName))
                .ForMember(dest => dest.Equals, opt => opt.MapFrom(src => src.Equals))
                .ForMember(dest => dest.GreaterThan, opt => opt.MapFrom(src => src.GreaterThan))
                .ForMember(dest => dest.GreaterThanOrEquals, opt => opt.MapFrom(src => src.GreaterThanOrEquals))
                .ForMember(dest => dest.LessThan, opt => opt.MapFrom(src => src.LessThan))
                .ForMember(dest => dest.LessThanOrEquals, opt => opt.MapFrom(src => src.LessThanOrEquals))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                ;

            // segment playback block action
            cfg.CreateMap<KalturaSegementAssetFilterAction, SegementAssetFilterAction>()
                .ForMember(dest => dest.Ksql, opt => opt.MapFrom(src => src.Ksql))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                ;

            cfg.CreateMap<SegementAssetFilterAction, KalturaSegementAssetFilterAction>()
                .ForMember(dest => dest.Ksql, opt => opt.MapFrom(src => src.Ksql))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src =>src.Type))
                ;

            cfg.CreateMap<KalturaSegementAssetFilterType, eTransactionType>()
              .ConvertUsing(type =>
              {
                  switch (type)
                  {
                      case KalturaSegementAssetFilterType.Subscription:
                          return eTransactionType.Subscription;
                      default:
                          throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown KalturaSegementAssetFilterType value : {0}", type.ToString()));
                  }
              });

            cfg.CreateMap<eTransactionType, KalturaSegementAssetFilterType>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case eTransactionType.Subscription:
                            return KalturaSegementAssetFilterType.Subscription;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown eTransactionType value : {0}", type.ToString()));
                    }
                });

            #endregion

            #region User Segment

            // User Segment
            cfg.CreateMap<KalturaUserSegment, UserSegment>()
                .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.UserId))
                .ForMember(d => d.SegmentId, opt => opt.MapFrom(s => s.SegmentId))
                ;

            cfg.CreateMap<UserSegment, KalturaUserSegment>()
                .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.UserId))
                .ForMember(d => d.SegmentId, opt => opt.MapFrom(s => s.SegmentId))
                ;

            #endregion

            #region Household Segment

            // User Segment
            cfg.CreateMap<KalturaHouseholdSegment, HouseholdSegment>()
                .ForMember(d => d.HouseholdId, opt => opt.MapFrom(s => s.HouseholdId))
                .ForMember(d => d.HouseholdSegmentId, opt => opt.MapFrom(s => s.HouseholdSegmentId))
                .ForMember(d => d.BlockingSegmentIds, opt => opt.MapFrom(s => s.GetBlockingSegmentIds()))
                ;

            cfg.CreateMap<HouseholdSegment, KalturaHouseholdSegment>()
                .ForMember(d => d.HouseholdId, opt => opt.MapFrom(s => s.HouseholdId))
                .ForMember(d => d.HouseholdSegmentId, opt => opt.MapFrom(s => s.HouseholdSegmentId))
                .ForMember(d => d.BlockingSegmentIds, opt => opt.MapFrom(s => string.Join(",", s.BlockingSegmentIds)))
                ;

            #endregion
        }

        #region Private Convertors
        private static ContentConditionLengthType? ConvertLengthType(KalturaContentActionConditionLengthType? lengthType)
        {
            ContentConditionLengthType? result = null;

            if (lengthType.HasValue)
            {
                switch (lengthType)
                {
                    case KalturaContentActionConditionLengthType.minutes:
                        {
                            result = ContentConditionLengthType.minutes;
                            break;
                        }
                    case KalturaContentActionConditionLengthType.percentage:
                        {
                            result = ContentConditionLengthType.percentage;
                            break;
                        }
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown ContentConditionLengthType");
                        break;
                }
            }

            return result;
        }

        private static KalturaContentActionConditionLengthType? ConvertLengthType(ContentConditionLengthType? lengthType)
        {
            KalturaContentActionConditionLengthType? result = null;

            if (lengthType.HasValue)
            {
                switch (lengthType)
                {
                    case ContentConditionLengthType.minutes:
                        {
                            result = KalturaContentActionConditionLengthType.minutes;
                            break;
                        }
                    case ContentConditionLengthType.percentage:
                        {
                            result = KalturaContentActionConditionLengthType.percentage;
                            break;
                        }
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown ContentConditionLengthType");
                        break;
                }
            }

            return result;
        }

        private static ContentAction ConvertContentAction(KalturaContentAction action)
        {
            ContentAction result;

            switch (action)
            {
                case KalturaContentAction.watch_linear:
                    {
                        result = ContentAction.watch_linear;
                        break;
                    }
                case KalturaContentAction.watch_vod:
                    {
                        result = ContentAction.watch_vod;
                        break;
                    }
                case KalturaContentAction.catchup:
                    {
                        result = ContentAction.catchup;
                        break;
                    }
                case KalturaContentAction.npvr:
                    {
                        result = ContentAction.npvr;
                        break;
                    }
                case KalturaContentAction.favorite:
                    {
                        result = ContentAction.favorite;
                        break;
                    }
                case KalturaContentAction.recording:
                    {
                        result = ContentAction.recording;
                        break;
                    }
                case KalturaContentAction.social_action:
                    {
                        result = ContentAction.social_action;
                        break;
                    }
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown ContentAction");
                    break;
            }

            return result;
        }

        private static KalturaContentAction ConvertContentAction(ContentAction action)
        {
            KalturaContentAction result;

            switch (action)
            {
                case ContentAction.watch_linear:
                    {
                        result = KalturaContentAction.watch_linear;
                        break;
                    }
                case ContentAction.watch_vod:
                    {
                        result = KalturaContentAction.watch_vod;
                        break;
                    }
                case ContentAction.catchup:
                    {
                        result = KalturaContentAction.catchup;
                        break;
                    }
                case ContentAction.npvr:
                    {
                        result = KalturaContentAction.npvr;
                        break;
                    }
                case ContentAction.favorite:
                    {
                        result = KalturaContentAction.favorite;
                        break;
                    }
                case ContentAction.recording:
                    {
                        result = KalturaContentAction.recording;
                        break;
                    }
                case ContentAction.social_action:
                    {
                        result = KalturaContentAction.social_action;
                        break;
                    }
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown ContentAction");
                    break;
            }

            return result;
        }

        private static MonetizationType ConvertMonetizationType(KalturaMonetizationType type)
        {
            MonetizationType result;

            switch (type)
            {
                case KalturaMonetizationType.ppv:
                    result = MonetizationType.ppv;
                    break;
                case KalturaMonetizationType.subscription:
                    result = MonetizationType.subscription;
                    break;
                case KalturaMonetizationType.boxset:
                    result = MonetizationType.boxset;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown MonetizationType");
                    break;
            }

            return result;
        }

        private static KalturaMonetizationType ConvertMonetizationType(MonetizationType type)
        {
            KalturaMonetizationType result;

            switch (type)
            {
                case MonetizationType.ppv:
                    result = KalturaMonetizationType.ppv;
                    break;
                case MonetizationType.subscription:
                    result = KalturaMonetizationType.subscription;
                    break;
                case MonetizationType.boxset:
                    result = KalturaMonetizationType.boxset;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown MonetizationType");
                    break;
            }

            return result;
        }

        private static MathemticalOperatorType ConvertMathematicalOperator(KalturaMathemticalOperatorType operatorType)
        {
            MathemticalOperatorType result;

            switch (operatorType)
            {
                case KalturaMathemticalOperatorType.count:
                    result = MathemticalOperatorType.count;
                    break;
                case KalturaMathemticalOperatorType.sum:
                    result = MathemticalOperatorType.sum;
                    break;
                case KalturaMathemticalOperatorType.avg:
                    result = MathemticalOperatorType.avg;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown MathemticalOperatorType");
                    break;
            }

            return result;
        }

        private static KalturaMathemticalOperatorType ConvertMathematicalOperator(MathemticalOperatorType operatorType)
        {
            KalturaMathemticalOperatorType result;

            switch (operatorType)
            {
                case MathemticalOperatorType.count:
                    result = KalturaMathemticalOperatorType.count;
                    break;
                case MathemticalOperatorType.sum:
                    result = KalturaMathemticalOperatorType.sum;
                    break;
                case MathemticalOperatorType.avg:
                    result = KalturaMathemticalOperatorType.avg;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown MathemticalOperatorType");                    
            }

            return result;
        }

        private static KalturaBlockPlaybackType ConvertToSegmentBlockPlaybackActionType(eTransactionType type)
        {
            switch (type)
            {
                case eTransactionType.PPV:
                    return KalturaBlockPlaybackType.PPV;
                case eTransactionType.Subscription:
                    return KalturaBlockPlaybackType.Subscription;
                case eTransactionType.Collection:
                    return KalturaBlockPlaybackType.Boxet;
                default:
                    throw new NotImplementedException();
            }
        }

        private static eTransactionType ConvertFromSegmentBlockPlaybackActionType(KalturaBlockPlaybackType type)
        {
            switch (type)
            {
                case KalturaBlockPlaybackType.Subscription:
                    return eTransactionType.Subscription;
                case KalturaBlockPlaybackType.PPV:
                    return eTransactionType.PPV;
                case KalturaBlockPlaybackType.Boxet:
                    return eTransactionType.Collection;
                default:
                    throw new NotImplementedException();
            }
        }
        #endregion
    }
}
