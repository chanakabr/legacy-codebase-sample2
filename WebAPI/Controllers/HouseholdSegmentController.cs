using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Segmentation;
using System;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Segmentation;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("householdSegment")]
    [AddAction(Summary = "householdSegment add",
               ObjectToAddDescription = "householdSegment details",
               ClientThrows = new eResponseStatus[]{
                   eResponseStatus.HouseholdRequired,
                   eResponseStatus.ObjectNotExist
               })]

    [DeleteAction(Summary = "Remove segment from household",
                  IdDescription = "Segment identifier",
                  ClientThrows = new eResponseStatus[] { 
                      eResponseStatus.HouseholdRequired,
                      eResponseStatus.ObjectNotExist,
                  })]              

    [ListAction(Summary = "Gets all HouseholdSegment items for a household", IsFilterOptional = true)]
    public class HouseholdSegmentController : KalturaCrudController<KalturaHouseholdSegment, KalturaHouseholdSegmentListResponse, HouseholdSegment, long, KalturaHouseholdSegmentFilter, HouseholdSegmentFilter>
    {
    }    
}