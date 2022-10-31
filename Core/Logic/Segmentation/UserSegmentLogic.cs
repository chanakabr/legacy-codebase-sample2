using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects.CanaryDeployment.Microservices;
using OTT.Service.Segmentation;
using SegmentationGrpcClientWrapper;

namespace ApiLogic.Segmentation
{
    public class UserSegmentLogic
    {
        #region Public methods

        public static List<long> List(int groupId, string userId, out int totalCount, List<long> segmentsIds = null)
        {
            List<long> result;

            if(CanaryDeploymentManager.CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.Segmentation))
            {

                var segmentIds = SegmentationClient.Instance.ListUserSegmentIds(new ListUserSegmentRequest
                {
                    PartnerId = groupId,
                    UserId = long.Parse(userId)
                });

                result = segmentIds.ToList();
                
            } else {
                
                var userSegments = ApiObjects.Segmentation.UserSegment.ListFromCb(groupId, userId, out totalCount, segmentsIds);
                result = userSegments.Select(i => i.SegmentId).ToList();
                
            }
            
            totalCount = result.Count();
            return result;
        }

        public static HashSet<long> ListAll(int groupId, string userId)
        {
            var response = new HashSet<long>();
            if(CanaryDeploymentManager.CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.Segmentation))
            {
                var segmentIds = SegmentationClient.Instance.ListUserSegmentIds(new ListUserSegmentRequest
                {
                    PartnerId = groupId,
                    UserId = long.Parse(userId)
                });

                response = new HashSet<long>((IEnumerable<long>) segmentIds);
            }
            else
            {
                var userSegments = ApiObjects.Segmentation.UserSegment.ListAllFromCb(groupId, userId);
                if (userSegments != null)
                    response = new HashSet<long>(userSegments.Select(i => i.SegmentId));
            }

            return response;
        }

        #endregion
    }
}
