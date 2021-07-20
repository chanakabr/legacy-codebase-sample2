using ApiObjects.Response;
using ApiObjects.Segmentation;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Segmentation;

namespace WebAPI.Controllers
{
    [Service("householdSegment")]
    [AddAction(Summary = "householdSegment add",
               ObjectToAddDescription = "householdSegment details",
               ClientThrows = new [] {
                   eResponseStatus.HouseholdRequired,
                   eResponseStatus.DomainNotExists,
                   eResponseStatus.ObjectNotExist
               })]

    [DeleteAction(Summary = "Remove segment from household",
                  IdDescription = "Segment identifier",
                  ClientThrows = new [] { 
                      eResponseStatus.HouseholdRequired,
                      eResponseStatus.ObjectNotExist,
                  })]              

    [ListAction(Summary = "Gets all HouseholdSegment items for a household", IsFilterOptional = true)]
    public class HouseholdSegmentController : KalturaCrudController<KalturaHouseholdSegment, KalturaHouseholdSegmentListResponse, HouseholdSegment, long, KalturaHouseholdSegmentFilter>
    {
    }    
}