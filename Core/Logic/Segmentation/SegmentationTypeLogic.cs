using ApiObjects.CanaryDeployment.Microservices;
using ApiObjects.Segmentation;
using OTT.Service.Segmentation;
using SegmentationGrpcClientWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SegmentationType = ApiObjects.Segmentation.SegmentationType;

namespace ApiLogic.Segmentation
{
    public interface ISegmentationTypeLogic
    {
        List<SegmentationType> ListBySegmentIds(int groupId, List<long> segmentIds, int pageIndex, int pageSize, out int totalCount, long userId, List<long> userRoleIds);
    }

    public class SegmentationTypeLogic : ISegmentationTypeLogic
    {
        private static readonly Lazy<SegmentationTypeLogic> LazyInstance =
            new Lazy<SegmentationTypeLogic>(() => new SegmentationTypeLogic(), LazyThreadSafetyMode.PublicationOnly);

        public static readonly ISegmentationTypeLogic Instance = LazyInstance.Value;

        private SegmentationTypeLogic()
        {
        }

        public List<SegmentationType> ListBySegmentIds(int groupId, List<long> segmentIds, int pageIndex, int pageSize, out int totalCount, long userId, List<long> userRoleIds)
        {
            List<SegmentationType> result = new List<SegmentationType>();
            totalCount = 0;
            if(CanaryDeploymentManager.CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager()
                .IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.Segmentation))
            {
                var request = new GetSegmentationTypesByValueRequest
                {
                    PartnerId = groupId,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    UserId = userId
                };
                if (segmentIds != null && segmentIds.Any()) request.Ids.AddRange(segmentIds);
                if (userRoleIds != null && userRoleIds.Any()) request.RoleIds.AddRange(userRoleIds);

                var response = SegmentationClient.Instance.GetSegmentationTypesByValue(request);
                result = response.Item1;
                totalCount = response.Item2;
            }
            else
            {
                result = SegmentationType.ListFromCb(groupId, segmentIds, pageIndex, pageSize, out totalCount);
            }

            if (result == null)
            {
                throw new Exception("Failed getting list of segmentation types from Couchbase");
            }

            return result;
        }

        public static List<SegmentationType> ListActionOfType<T>(int groupId, List<long> ids) where T : SegmentAction
        {
            List<SegmentationType> segmentations;

            if (CanaryDeploymentManager.CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager()
                .IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.Segmentation))
            {
                var response = SegmentationClient.Instance.GetSegmentationTypesByValue(new GetSegmentationTypesByValueRequest
                {
                    PartnerId = groupId
                });
                segmentations = response.Item1;
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
                var response = SegmentationClient.Instance.GetSegmentationTypesByValue(new GetSegmentationTypesByValueRequest
                {
                    PartnerId = groupId,
                    Ids = { segmentIds }
                });
                return response.Item1;
            }
            else
            {
                return SegmentationType.GetSegmentationTypesBySegmentIdsFromCb(groupId, segmentIds);
            }
        }
    }
}