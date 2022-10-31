using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Protobuf.Collections;
using Grpc.Core;
using MoreLinq;
using OTT.Service.Segmentation;
using Segmentation = ApiObjects.Segmentation;
using SegmentationTypeObject = ApiObjects.Segmentation.SegmentationType;

namespace SegmentationGrpcClientWrapper
{
    public static class SegmentationMapper
    {
        public static List<SegmentationTypeObject> MapToListResponse(GetSegmentationTypesResponse response) =>
            response.Objects?.Select(MapToSegmentationType).ToList();

        public static SegmentationTypeObject MapToSegmentationType(SegmentationType source) =>
            new SegmentationTypeObject
            {
                Id = source.Id,
                Name = source.Name,
                Description = source.Description,
                Conditions = MapToConditions(source.Conditions),
                Actions = MapToActions(source.Actions),
                Value = MapToSingleSegmentValue(source.Value),
                CreateDate = source.CreateDate,
                Version = source.Version,
            };
        
        private static List<Segmentation.SegmentAction> MapToActions(RepeatedField<OTT.Service.Segmentation.Action> source)
        {
            if (source == null) { return null; }

            var actions = new List<Segmentation.SegmentAction>();
            source.ForEach(action =>
            {
                if (action.AssetOrderSegmentAction != null)
                {
                    actions.Add(MapToAssetOrderSegmentAction((AssetOrderSegmentAction) action.AssetOrderSegmentAction));
                }
                else if (action.KsqlSegmentAction != null)
                {
                    switch (action.KsqlSegmentAction.Type)
                    {
                        case KsqlSegmentActionType.SegmentBlockCancelSubscriptionAction:
                            actions.Add(new Segmentation.SegmentBlockCancelSubscriptionAction { Ksql = action.KsqlSegmentAction.Ksql });
                            break;
                        case KsqlSegmentActionType.SegmentBlockPlaybackSubscriptionAction:
                            actions.Add(new Segmentation.SegmentBlockPlaybackSubscriptionAction{ Ksql = action.KsqlSegmentAction.Ksql });
                            break;
                        case KsqlSegmentActionType.SegmentBlockPurchaseSubscriptionAction:
                            actions.Add(new Segmentation.SegmentBlockPurchaseSubscriptionAction { Ksql = action.KsqlSegmentAction.Ksql });
                            break;
                        case KsqlSegmentActionType.SegmentAssetFilterSegmentAction:
                            actions.Add(new Segmentation.SegmentAssetFilterSegmentAction { Ksql = action.KsqlSegmentAction.Ksql });
                            break;
                        case KsqlSegmentActionType.SegmentAssetFilterSubscriptionAction:
                            actions.Add(new Segmentation.SegmentAssetFilterSubscriptionAction { Ksql = action.KsqlSegmentAction.Ksql });
                            break;
                        default:
                            throw new EvaluateException( $"KsqlSegmentAction [{action.KsqlSegmentAction.Type}] is not supported");
                    }
                }
                else
                {
                    throw new InvalidDataException($"Action [{action.GetType().Name}] is not supported");
                }
            });

            return actions;
        }

        private static Segmentation.SegmentAssetOrderAction MapToAssetOrderSegmentAction(AssetOrderSegmentAction source) =>
            new Segmentation.SegmentAssetOrderAction
            {
                Name = source.Name,
                Values = source.Values.ToList()
            };

        private static Segmentation.SegmentDummyValue MapToSingleSegmentValue(Value value) =>
            new Segmentation.SegmentDummyValue
            {
                Id = value.Id,
                AffectedUsers = value.AffectedUsers,
                AffectedHouseholds = value.AffectedHouseholds
            };

        private static List<Segmentation.SegmentCondition> MapToConditions(Conditions source)
        {
            if (source == null) { return null; }

            var conditions = new List<Segmentation.SegmentCondition>();

            if (source.ContentScoreConditions?.Count > 0)
            {
                conditions.AddRange(source.ContentScoreConditions.Select(MapToContentScoreCondition));
            }

            if (source.MonetizationConditions?.Count > 0)
            {
                conditions.AddRange(source.MonetizationConditions.Select(MapToMonetizationCondition));
            }

            if (source.UserDataConditions?.Count > 0)
            {
                conditions.AddRange(source.UserDataConditions.Select(MapToUserDataCondition));
            }

            return conditions;
        }

        private static Segmentation.ContentScoreCondition MapToContentScoreCondition(ContentScoreCondition source) =>
            new Segmentation.ContentScoreCondition
            {
                Field = source.Field,
                Days = source.Days,
                MaxScore = source.MaxScore,
                MinScore = source.MinScore,
                Actions = source.Actions?.Select(MapToContentActionCondition).ToList(),
                Values = source.Values.ToList()
            };

        private static Segmentation.ContentActionCondition MapToContentActionCondition(ContentActionCondition source) =>
            new Segmentation.ContentActionCondition
            {
                Action = Map(source.Action),
                Length = source.Length,
                LengthType = Map(source.LengthType),
                Multiplier = source.Multiplier
            };
        
        private static Segmentation.SegmentUserDataCondition MapToUserDataCondition(UserDataCondition source) =>
            new Segmentation.SegmentUserDataCondition
            {
                Field = source.Field,
                Value = source.Value
            };
        
        private static Segmentation.MonetizationCondition MapToMonetizationCondition(MonetizationCondition source) =>
            new Segmentation.MonetizationCondition
            {
                MinValue = source.MinValue,
                MaxValue = source.MaxValue,
                Days = source.Days,
                Type = Map(source.Type),
                Operator = Map(source.Operator),
                BusinessModuleIds = source.BusinessModules.Select(i => (int)i).ToList(),
                CurrencyCode = source.CurrencyCode,
            };
        
        private static Segmentation.MathemticalOperatorType Map(MathematicalOperator source)
        {
            switch (source)
            {
                case MathematicalOperator.Count:return Segmentation.MathemticalOperatorType.count;
                case MathematicalOperator.Sum: return Segmentation.MathemticalOperatorType.sum;
                case MathematicalOperator.Avg: return Segmentation.MathemticalOperatorType.avg;
                default:
                    throw new EvaluateException($"MathematicalOperator [{source}] is not supported");
            }
        }

        private static Segmentation.MonetizationType Map(MonetizationType source)
        {
            switch (source)
            {
                case MonetizationType.Ppv: return Segmentation.MonetizationType.ppv;
                case MonetizationType.Subscription: return Segmentation.MonetizationType.subscription;
                case MonetizationType.BoxSet: return Segmentation.MonetizationType.boxset;
                case MonetizationType.Any: return Segmentation.MonetizationType.any;
                case MonetizationType.Ppvlive: return Segmentation.MonetizationType.ppv_live;
                default:
                    throw new EvaluateException($"MonetizationType [{source}] is not supported");
            }
        }

        public static Segmentation.ContentAction Map(ContentAction source)
        {
            switch (source)
            {
                case ContentAction.WatchLinear: return Segmentation.ContentAction.watch_linear;
                case ContentAction.WatchVod: return Segmentation.ContentAction.watch_vod;
                case ContentAction.Catchup: return Segmentation.ContentAction.catchup;
                case ContentAction.Npvr: return Segmentation.ContentAction.npvr;
                case ContentAction.Favorite: return Segmentation.ContentAction.favorite;
                case ContentAction.Recording: return Segmentation.ContentAction.recording;
                case ContentAction.SocialAction: return Segmentation.ContentAction.social_action;
                default:
                    throw new EvaluateException($"ContentAction [{source}] is not supported");
            }
        }


        public static Segmentation.ContentConditionLengthType? Map(ContentActionConditionLengthType source)
        {
            switch (source)
            {
                case ContentActionConditionLengthType.Minutes: return Segmentation.ContentConditionLengthType.minutes;
                case ContentActionConditionLengthType.Percentage: return Segmentation.ContentConditionLengthType.percentage;
                case ContentActionConditionLengthType.Null: return null;
                default:
                    throw new EvaluateException($"ContentActionConditionLengthType [{source}] is not supported");
            }
        }

    }
}