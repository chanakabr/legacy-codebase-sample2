using ApiObjects.Response;
using CachingProvider.LayeredCache;
using CouchbaseManager;
using Phx.Lib.Log;
using Phx.Lib.Appconfig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiObjects.CanaryDeployment.Microservices;
using ApiObjects.Segmentation;
using OTT.Service.Segmentation;
using SegmentationGrpcClientWrapper;
using SegmentationType = ApiObjects.Segmentation.SegmentationType;

namespace ApiLogic.Segmentation
{
    public class SegmentationTypeLogic
    {
        public static List<SegmentationType> List(int groupId, List<long> ids, int pageIndex, int pageSize, out int totalCount)
        {
            List<SegmentationType> result = new List<SegmentationType>();
            totalCount = 0;
            if(CanaryDeploymentManager.CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.Segmentation))
            {

                result = SegmentationClient.Instance.GetSegmentationTypesByValue(new GetSegmentationTypesByValueRequest
                {
                    PartnerId = groupId,
                    Ids = { ids },
                    PageIndex = pageIndex,
                    PageSize = pageSize
                });

                totalCount = result.Count;
                
            }
            else
            {
                result = SegmentationType.ListFromCb(groupId, ids, pageIndex, pageSize, out totalCount);
            }

            DateTime now = DateTime.UtcNow;

            if (result == null)
            {
                throw new Exception("Failed getting list of segmentation types from Couchbase");
            }

            return result;
        }

        public static List<ApiObjects.Segmentation.SegmentationType> ListActionOfType<T>(int groupId, List<long> ids) where T : SegmentAction
        {
            List<ApiObjects.Segmentation.SegmentationType> segmentations;
            
            if(CanaryDeploymentManager.CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.Segmentation))
            {
                segmentations = SegmentationClient.Instance.GetSegmentationTypesByValue(new GetSegmentationTypesByValueRequest
                {
                    PartnerId = groupId
                });
            }
            else
            {
                segmentations = SegmentationType.ListActionOfTypeFromCb<T>(groupId, ids);
            }

            return segmentations;
        }

        public static List<SegmentationType> GetSegmentationTypesBySegmentIds(int groupId, IEnumerable<long> segmentIds)
        {
            if (CanaryDeploymentManager.CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager()
                .IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.Segmentation))
            {
                return SegmentationClient.Instance.GetSegmentationTypesByValue(new GetSegmentationTypesByValueRequest
                {
                    PartnerId = groupId,
                    Ids = {segmentIds}
                });
            }
            else
            {
                return SegmentationType.GetSegmentationTypesBySegmentIdsFromCb(groupId, segmentIds);
            }
        }
        
    }
}