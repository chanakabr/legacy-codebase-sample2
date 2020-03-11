using ApiObjects;
using ApiObjects.Pricing;
using ApiObjects.Response;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.Domains;

namespace WebAPI.Controllers
{
    [Service("eventNotification")]
    [UpdateAction(Summary = "eventNotification update", ObjectToUpdateDescription = "eventNotification details",
               ClientThrows = new eResponseStatus[]
               {
                   eResponseStatus.EventNotificationIdIsMissing,
                   eResponseStatus.EventNotificationIdNotFound
               })]
    [ListAction(Summary = "Gets all EventNotification items for a given Object id and type")]
    public class EventNotificationController : KalturaCrudController<KalturaEventNotification, KalturaEventNotificationListResponse, EventNotificationAction, string, KalturaEventNotificationFilter, EventNotificationActionFilter>
    {
    }
}