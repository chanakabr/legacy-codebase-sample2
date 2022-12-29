using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using OTT.Lib.Kafka;
using SchemaRegistryEvents.Catalog;
using Phoenix.Generated.Api.Events.Crud.SegmentationType;
using Microsoft.Extensions.Logging;
using System;
using ApiObjects;
using ApiObjects.CanaryDeployment.Microservices;
using CanaryDeploymentManager;
using Action = Phoenix.Generated.Api.Events.Crud.SegmentationType.Action;

namespace ApiLogic.Modules.Services
{
    public class SegmentationTypeCrudMessageService : ISegmentationTypeCrudMessageService
    {
        private readonly IKafkaProducer<string, SegmentationType> _segProducer;
        private readonly ILogger _logger;

        private enum ActionType
        {
            SegmentBlockCancelSubscriptionAction, SegmentBlockPlaybackSubscriptionAction,
            SegmentBlockPurchaseSubscriptionAction, SegmentAssetFilterSegmentAction,
            SegmentAssetFilterSubscriptionAction
        }

        public SegmentationTypeCrudMessageService(IKafkaProducerFactory producerFactory, IKafkaContextProvider contextProvider, ILogger logger)
        {
            _segProducer = producerFactory.Get<string, SegmentationType>(contextProvider, Partitioner.Murmur2Random);
            _logger = logger;
        }
        
        public Task PublishCreateEventAsync(int groupId, ApiObjects.Segmentation.SegmentationType segType) => PublishKafkaCreateOrUpdateEvent(groupId, CrudOperationType.CREATE_OPERATION, segType);

        public Task PublishUpdateEventAsync(int groupId, ApiObjects.Segmentation.SegmentationType segType) => PublishKafkaCreateOrUpdateEvent(groupId, CrudOperationType.UPDATE_OPERATION, segType);

        private Task PublishKafkaCreateOrUpdateEvent(long groupId, long operation, ApiObjects.Segmentation.SegmentationType source)
        {
            // TODO in question: also call migration event
            SegmentationType segmentationTypeEvent = MapBolToEvent(groupId, operation, source);
            return _segProducer.ProduceAsync(SegmentationType.GetTopic(), segmentationTypeEvent.GetPartitioningKey(), segmentationTypeEvent);
        }

        public Task PublishDeleteEventAsync(long groupId, long segTypeId)
        {
            var segmentationTypeEvent = new SegmentationType
            {
                Id = segTypeId,
                PartnerId = groupId,
                Operation = CrudOperationType.DELETE_OPERATION
            };

            return _segProducer.ProduceAsync(SegmentationType.GetTopic(), segmentationTypeEvent.GetPartitioningKey(), segmentationTypeEvent);
        }

        public Task PublishMigrationCreateEventAsync(long groupId, ApiObjects.Segmentation.SegmentationType segType)
        {
            var segmentationTypeEvent = MapBolToEvent(groupId, CrudOperationType.CREATE_OPERATION, segType);

            return PublishMigrationEventAsync(segmentationTypeEvent);
        }

        private Task PublishMigrationEventAsync(SegmentationType segmentationTypeEvent)
        {
            return _segProducer.ProduceAsync($"{SegmentationType.GetTopic()}.migration", segmentationTypeEvent.GetPartitioningKey(), segmentationTypeEvent);
        }

        #region Mapping

        private SegmentationType MapBolToEvent(long groupId, long operation, ApiObjects.Segmentation.SegmentationType source)
        {
            var segmentationTypeEvent = new SegmentationType()
            {
                CreateDate = source.CreateDate,
                Description = source.Description,
                ExecutionDate = source.ExecuteDate,
                Id = source.Id,
                Name = source.Name,
                Operation = operation,
                PartnerId = groupId,
                UpdateDate = source.UpdateDate,
                Version = source.Version,
            };

            if (source.Value is ApiObjects.Segmentation.SegmentDummyValue castedValue)
            {
                segmentationTypeEvent.Value = new Value()
                {
                    Id = castedValue.Id,
                    AffectedHouseholds = castedValue.AffectedHouseholds,
                    AffectedUsers = castedValue.AffectedUsers
                };
                segmentationTypeEvent.SegmentsValues = new[] { new SegmentsValue { Id = castedValue.Id } };
            }
            else if (source.Value is ApiObjects.Segmentation.SegmentValues segmentValues)
            {
                segmentationTypeEvent.SegmentsValues = segmentValues.Values.Select(curr => new SegmentsValue
                { 
                    Id = curr.Id,
                    Name = curr.Name,
                    Value = curr.Value
                })
                .ToArray();
            }

            if (source.Conditions?.Count > 0)
            {
                segmentationTypeEvent.Conditions = ConvertConditionsToEvent(source);
            }

            if (source.Actions?.Count > 0)
            {
                segmentationTypeEvent.Actions = ConvertActionsToEvent(source);
            }

            return segmentationTypeEvent;
        }

        private Action[] ConvertActionsToEvent(ApiObjects.Segmentation.SegmentationType source)
        {
            return source.Actions.Select(action => MapToAction(action)).ToArray();
        }

        private static Phoenix.Generated.Api.Events.Crud.SegmentationType.Action MapToAction(ApiObjects.Segmentation.SegmentAction source)
        {
            switch (source)
            {
                case ApiObjects.Segmentation.SegmentAssetOrderAction a: return MapToAssetOrderSegmentAction(a);
                case ApiObjects.Segmentation.SegmentBlockCancelSubscriptionAction a: return MapToKsqlSegmentAction(ActionType.SegmentBlockCancelSubscriptionAction, a.Ksql);
                case ApiObjects.Segmentation.SegmentBlockPlaybackSubscriptionAction a: return MapToKsqlSegmentAction(ActionType.SegmentBlockPlaybackSubscriptionAction, a.Ksql);
                case ApiObjects.Segmentation.SegmentBlockPurchaseSubscriptionAction a: return MapToKsqlSegmentAction(ActionType.SegmentBlockPurchaseSubscriptionAction, a.Ksql);
                case ApiObjects.Segmentation.SegmentAssetFilterSegmentAction a: return MapToKsqlSegmentAction(ActionType.SegmentAssetFilterSegmentAction, a.Ksql);
                case ApiObjects.Segmentation.SegmentAssetFilterSubscriptionAction a: return MapToKsqlSegmentAction(ActionType.SegmentAssetFilterSubscriptionAction, a.Ksql);
                default:
                    return null;
            }
        }

        private static Phoenix.Generated.Api.Events.Crud.SegmentationType.Action MapToKsqlSegmentAction(ActionType type, string ksql) =>
            new Phoenix.Generated.Api.Events.Crud.SegmentationType.Action
            {
                KsqlSegmentAction = new KsqlSegmentAction()
                {
                    ActionType = type.ToString(),
                    Ksql = ksql,
                }
            };

        private static Action MapToAssetOrderSegmentAction(ApiObjects.Segmentation.SegmentAssetOrderAction source)
        {
            var action = new Action
            {
                AssetOrderSegmentAction = new AssetOrderSegmentAction
                {
                    Name = source.Name,
                }
            };

            if (source.Values?.Count > 0)
            {
                action.AssetOrderSegmentAction.Values = source.Values?.ToArray();
            }

            return action;
        }

        private Conditions ConvertConditionsToEvent(ApiObjects.Segmentation.SegmentationType source)
        {
            ContentScoreCondition[] contentScoreConditions = source.Conditions.OfType<ApiObjects.Segmentation.ContentScoreCondition>().Select(condition => new ContentScoreCondition()
            {
                Days = condition.Days,
                Field = condition.Field,
                MinScore = condition.MinScore,
                MaxScore = condition.MaxScore,
                Values = condition.Values.ToArray(),
                ContentActions = condition.Actions.Select(contentAction => new ContentAction()
                {
                    Action = contentAction.Action.ToString(),
                    Length = contentAction.Length,
                    LengthType = contentAction.LengthType?.ToString(),
                    Multiplier = contentAction.Multiplier
                }).ToArray()
            }).ToArray();

            MonetizationCondition[] monetizationConditions = source.Conditions.OfType<ApiObjects.Segmentation.MonetizationCondition>().Select(condition => new MonetizationCondition()
            {
                BusinessModules = condition.BusinessModuleIds.Cast<long>().ToArray(),
                CurrencyCode = condition.CurrencyCode,
                Days = condition.Days,
                MaxValue = condition.MaxValue,
                MinValue = condition.MinValue,
                MonetizationType = condition.Type.ToString(),
                Operator = condition.Operator.ToString()
            }).ToArray();

            UserDataCondition[] userDataConditions = source.Conditions.OfType<ApiObjects.Segmentation.SegmentUserDataCondition>().Select(condition => new UserDataCondition()
            {
                Field = condition.Field,
                Value = condition.Value
            }).ToArray();

            var conditions = new Conditions()
            {
                ContentScoreConditions = contentScoreConditions,
                UserDataConditions = userDataConditions,
                MonetizationConditions = monetizationConditions,
                Operator = source.ConditionsOperator.ToString()
            };
            return conditions;
        }

        // this code is commented because ideally (?) enums are properly generated from schema registry.

        //private MonetizationConditionOperator? ConvertMonetiationOperatorEnum(ApiObjects.Segmentation.MathemticalOperatorType sourceOperator)
        //{
        //    switch (sourceOperator)
        //    {
        //        case ApiObjects.Segmentation.MathemticalOperatorType.count:
        //            return MonetizationConditionOperator.Count;
        //        case ApiObjects.Segmentation.MathemticalOperatorType.sum:
        //            return MonetizationConditionOperator.Sum;
        //        case ApiObjects.Segmentation.MathemticalOperatorType.avg:
        //            return MonetizationConditionOperator.Avg;
        //        default:
        //            break;
        //    }

        //    return null;
        //}

        //private MonetizationType? ConvertMoneitzationType(ApiObjects.Segmentation.MonetizationType type)
        //{
        //    switch (type)
        //    {
        //        case ApiObjects.Segmentation.MonetizationType.ppv:
        //            return MonetizationType.Ppv;
        //        case ApiObjects.Segmentation.MonetizationType.subscription:
        //            return MonetizationType.Subscription;
        //        case ApiObjects.Segmentation.MonetizationType.boxset:
        //            return MonetizationType.BoxSet;
        //        case ApiObjects.Segmentation.MonetizationType.any:
        //            return MonetizationType.Any;
        //        case ApiObjects.Segmentation.MonetizationType.ppv_live:
        //            return MonetizationType.PpvLive;
        //        default:
        //            break;
        //    }

        //    return null;
        //}

        //private ConditionsOperator? ConvertConditionsOperator(eCutType conditionsOperator)
        //{
        //    switch (conditionsOperator)
        //    {
        //        case eCutType.And:
        //            return ConditionsOperator.And;
        //        case eCutType.Or:
        //            return ConditionsOperator.Or;
        //        default:
        //            break;
        //    }

        //    return null;
        //}

        //private LengthType? ConvertLengthTypeEnum(ApiObjects.Segmentation.ContentConditionLengthType? lengthType)
        //{
        //    switch (lengthType)
        //    {
        //        case ApiObjects.Segmentation.ContentConditionLengthType.percentage:
        //            return LengthType.Percentage;
        //        case ApiObjects.Segmentation.ContentConditionLengthType.minutes:
        //            return LengthType.Percentage;
        //        default:
        //            break;
        //    }

        //    return null;
        //}

        //private ActionEnum? ConvertActionEnum(ApiObjects.Segmentation.ContentAction action)
        //{
        //    switch (action)
        //    {
        //        case ApiObjects.Segmentation.ContentAction.watch_linear:
        //            return ActionEnum.WatchLinear;
        //        case ApiObjects.Segmentation.ContentAction.watch_vod:
        //            return ActionEnum.WatchVod;
        //        case ApiObjects.Segmentation.ContentAction.catchup:
        //            return ActionEnum.Catchup;
        //        case ApiObjects.Segmentation.ContentAction.npvr:
        //            return ActionEnum.Npvr;
        //        case ApiObjects.Segmentation.ContentAction.favorite:
        //            return ActionEnum.Favorite;
        //        case ApiObjects.Segmentation.ContentAction.recording:
        //            return ActionEnum.Recording;
        //        case ApiObjects.Segmentation.ContentAction.social_action:
        //            return ActionEnum.SocialAction;
        //        default:
        //            break;
        //    }
        //    return null;
        //}


        #endregion
    }
}