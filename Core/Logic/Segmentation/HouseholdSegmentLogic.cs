using System.Collections.Generic;
using System.Linq;
using ApiObjects.CanaryDeployment.Microservices;
using OTT.Service.Segmentation;
using SegmentationGrpcClientWrapper;

namespace ApiLogic.Segmentation
{
    public class HouseholdSegmentLogic
    {
        public static HashSet<long> List(int groupId, long householdId, out int totalCount, List<long> segmentsIds = null)
        {
            totalCount = 0;
            HashSet<long> result = new HashSet<long>();

            if(CanaryDeploymentManager.CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.Segmentation))
            {
                var segmentIds = SegmentationClient.Instance.ListHouseholdSegmentIds(new ListHouseholdSegmentRequest()
                {
                    PartnerId = groupId,
                    HouseholdId = householdId
                });

                result =  new HashSet<long>((IEnumerable<long>) segmentIds);
                totalCount = result.Count;

            } else {
                
                var householdSegments = ApiObjects.Segmentation.HouseholdSegment.ListFromCb(groupId, householdId, out totalCount, segmentsIds);
                result = new HashSet<long>(householdSegments.Select(i => i.SegmentId));
                
            }

            return result;
        }
    }
}