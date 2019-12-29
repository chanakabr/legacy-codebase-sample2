using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using ApiObjects.Segmentation;
using Core.Api;
using KLogMonitor;
using System;
using System.Reflection;

namespace ApiLogic.Users.Managers
{
    public class HouseholdSegmentManager : ICrudHandler<HouseholdSegment, long, HouseholdSegmentFilter>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<HouseholdSegmentManager> lazy = new Lazy<HouseholdSegmentManager>(() => new HouseholdSegmentManager());

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

                objectToAdd.GroupId = contextData.GroupId;
                var assetSearchDefinition = new AssetSearchDefinition() { UserId = contextData.UserId.Value };

                var filter = api.GetObjectVirtualAssetObjectIds(objectToAdd.GroupId, assetSearchDefinition, ObjectVirtualAssetInfoType.Segment, new System.Collections.Generic.HashSet<long>() { objectToAdd.SegmentId });
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
                    return response;
                }
                response.Object = objectToAdd;
                response.Status.Set(eResponseStatus.OK);

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

                HouseholdSegment householdSegment = new HouseholdSegment()
                {
                    HouseholdId = contextData.DomainId.Value,
                    SegmentId = segmentId,
                    GroupId = contextData.GroupId
                };

                if (!householdSegment.Delete())
                {
                    log.Error($"Error while delete HouseholdSegment. contextData: {contextData.ToString()}.");
                    response.Set(eResponseStatus.Error);
                    return response;
                }

                response.Set(eResponseStatus.OK);
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

                AssetSearchDefinition assetSearchDefinition = new AssetSearchDefinition() { UserId = contextData.UserId.Value, Filter = filter.Ksql };

                var filtered = api.GetObjectVirtualAssetObjectIds(contextData.GroupId, assetSearchDefinition, ObjectVirtualAssetInfoType.Segment);
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

                response.Objects = HouseholdSegment.List(contextData.GroupId, contextData.DomainId.Value, out int totalCount, filtered.ObjectIds);
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in HouseholdSegment List. domainId:{ contextData.DomainId.Value} . ex: {ex}");
            }

            return response;
        }

        public GenericResponse<HouseholdSegment> Update(ContextData contextData, HouseholdSegment objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<HouseholdSegment> Get(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }
    }
}