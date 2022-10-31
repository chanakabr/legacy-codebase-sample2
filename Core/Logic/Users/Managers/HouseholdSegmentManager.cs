using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using ApiObjects.Segmentation;
using Core.Api;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiLogic.Modules.Services;
using EventBus.Kafka;
using OTT.Lib.Kafka;

namespace ApiLogic.Users.Managers
{
    public class HouseholdSegmentManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<HouseholdSegmentManager> lazy = new Lazy<HouseholdSegmentManager>(() => new HouseholdSegmentManager());
        
        private static IHouseholdSegmentCrudMessageService  _householdSegmentMessageService;

        public static HouseholdSegmentManager Instance { get { return lazy.Value; } }

        public GenericResponse<HouseholdSegment> Add(ContextData contextData, HouseholdSegment objectToAdd)
        {
            var response = new GenericResponse<HouseholdSegment>();

            try
            {
                if (objectToAdd.HouseholdId == 0)
                {
                    response.SetStatus(eResponseStatus.HouseholdRequired, "Household required");
                    return response;
                }

                var domainResponse = Core.Domains.Module.GetDomainInfo(contextData.GroupId, (int)objectToAdd.HouseholdId);
                if (!domainResponse.Status.IsOkStatusCode() || domainResponse.Domain == null)
                {
                    response.SetStatus(domainResponse.Status);
                    return response;
                }

                objectToAdd.GroupId = contextData.GroupId;
                var assetSearchDefinition = new AssetSearchDefinition() { UserId = contextData.UserId.Value };

                long segmentationTypeId = SegmentBaseValue.GetSegmentationTypeOfSegmentId(objectToAdd.SegmentId);
                if (segmentationTypeId == 0)
                {
                    response.SetStatus(eResponseStatus.ObjectNotExist, "Segment not exist");
                    return response;
                }

                var filter = api.Instance.GetObjectVirtualAssetObjectIds(objectToAdd.GroupId, assetSearchDefinition, ObjectVirtualAssetInfoType.Segment, new System.Collections.Generic.HashSet<long>() { segmentationTypeId });
                if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
                {
                    response.SetStatus(filter.Status);
                    return response;
                }

                if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.None)
                {
                    response.SetStatus(new Status((int)eResponseStatus.ObjectNotExist, "Object Not Exist"));
                    return response;
                }

                if (!objectToAdd.Insert())
                {
                    log.Error($"Error while Save HouseholdSegment. contextData: {contextData.ToString()}.");
                    response.SetStatus(objectToAdd.ActionStatus);
                    return response;
                }
                response.Object = objectToAdd;
                response.Status.Set(eResponseStatus.OK);
                _householdSegmentMessageService?.PublishCreateEventAsync(contextData.GroupId, objectToAdd).GetAwaiter().GetResult();

            }
            catch (Exception ex)
            {
                log.Error($"Error while Save HouseholdSegment. contextData: {contextData.ToString()}. ex: {ex}");
            }

            return response;           
        }

        public Status Delete(ContextData contextData, long segmentId)
        {
            Status response = new Status();
            try
            {
                // validate 
                if (!contextData.DomainId.HasValue)
                {
                    response.Set(eResponseStatus.HouseholdRequired, "Household required");
                    return response;
                }

                if (segmentId  == 0)
                {
                    response.Set(eResponseStatus.ObjectNotExist, "Segment identifier required");
                    return response;
                }

                long segmentationTypeId = SegmentBaseValue.GetSegmentationTypeOfSegmentId(segmentId);
                if (segmentationTypeId == 0)
                {
                    response.Set(eResponseStatus.ObjectNotExist, "Segment not exist");
                    return response;
                }

                var filter = api.Instance.GetObjectVirtualAssetObjectIds(contextData.GroupId, new AssetSearchDefinition() { UserId = contextData.UserId.Value }, 
                                            ObjectVirtualAssetInfoType.Segment, new System.Collections.Generic.HashSet<long>() { segmentationTypeId });

                if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
                {
                    response.Set(filter.Status);
                    return response;
                }

                if (filter.ResultStatus == ObjectVirtualAssetFilterStatus.None)
                {
                    response.Set(new Status((int)eResponseStatus.ObjectNotExist, "Object Not Exist"));
                    return response;
                }

                HouseholdSegment householdSegment = new HouseholdSegment()
                {
                    HouseholdId = contextData.DomainId.Value,
                    SegmentId = segmentId,
                    GroupId = contextData.GroupId
                };

                var status = householdSegment.IsSegmentExist();

                if(status.Code != (int)eResponseStatus.OK)
                {
                    log.Error($"Error at delete HouseholdSegment. contextData: {contextData.ToString()}.");
                    response.Set(status);
                    return response;
                }

                if (!householdSegment.Delete())
                {
                    log.Error($"Error while delete HouseholdSegment. contextData: {contextData.ToString()}.");
                    response.Set(eResponseStatus.Error);
                    return response;
                }

                response.Set(eResponseStatus.OK);
                _householdSegmentMessageService?.PublishDeleteEventAsync(contextData.GroupId, householdSegment).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                log.Error($"Error while Delete HouseholdSegment. contextData: {contextData.ToString()}. ex: {ex}");
                response.Set(eResponseStatus.Error);
            }

            return response;
        }       

        public GenericListResponse<HouseholdSegment> List(ContextData contextData, HouseholdSegmentFilter filter)
        {
            var response = new GenericListResponse<HouseholdSegment>();
            try
            {
                if (!contextData.DomainId.HasValue)
                {
                    response.SetStatus(eResponseStatus.HouseholdRequired, "Household required");
                    return response;
                }

                if (!contextData.UserId.HasValue)
                {
                    response.SetStatus(eResponseStatus.UserDoesNotExist, "User required");
                    return response;
                }

                var householdSegments = ApiLogic.Segmentation.HouseholdSegmentLogic.List(contextData.GroupId, contextData.DomainId.Value, out int totalCount);

                if (totalCount > 0)
                {
                    var segmentTypeIds = SegmentBaseValue.GetSegmentationTypeOfSegmentIds(householdSegments.ToList());
                    if (segmentTypeIds?.Count > 0)
                    {
                        AssetSearchDefinition assetSearchDefinition = new AssetSearchDefinition() { UserId = contextData.UserId.Value, Filter = filter.Ksql };

                        var filtered = api.Instance.GetObjectVirtualAssetObjectIds(contextData.GroupId, assetSearchDefinition, ObjectVirtualAssetInfoType.Segment, new HashSet<long>( segmentTypeIds.Values.ToList()));
                        if (filtered.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
                        {
                            response.SetStatus(filtered.Status);
                            return response;
                        }

                        if (filtered.ResultStatus == ObjectVirtualAssetFilterStatus.None)
                        {
                            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                            return response;
                        }

                        if (filtered.ObjectIds?.Count > 0)
                        {
                            response.Objects = new List<HouseholdSegment>();

                            foreach (var segmentId in householdSegments)
                            {
                                if (segmentTypeIds.ContainsKey(segmentId))
                                {
                                    if (filtered.ObjectIds.Contains(segmentTypeIds[segmentId]))
                                    {
                                        response.Objects.Add(new HouseholdSegment{HouseholdId = contextData.DomainId.Value, GroupId = contextData.GroupId, SegmentId = segmentId});
                                    }
                                }
                            }
                        }
                    }
                }

                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in HouseholdSegment List. domainId:{ contextData.DomainId.Value} . ex: {ex}");
            }

            return response;
        }

        public GenericResponse<HouseholdSegment> Get(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }
        
        public static void InitHouseholdSegmentCrudMessageService(IKafkaContextProvider contextProvider)
        {
            _householdSegmentMessageService = new HouseholdSegmentCrudMessageService(
                KafkaProducerFactoryInstance.Get(),
                contextProvider,
                new KLogger(nameof(HouseholdSegmentCrudMessageService)));
        }
    }
}