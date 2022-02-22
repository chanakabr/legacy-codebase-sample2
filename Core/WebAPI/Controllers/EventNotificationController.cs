using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using System;
using WebAPI.Clients;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.ModelsValidators;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("eventNotification")]
    public class EventNotificationController : IKalturaController
    {
        /// <summary>
        /// eventNotification update
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">id of eventNotification</param>
        /// <param name="objectToUpdate">eventNotification details</param>
        [Action("update")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLength = 1)]
        [Throws(eResponseStatus.EventNotificationIdIsMissing)]
        static public KalturaEventNotification Update(string id, KalturaEventNotification objectToUpdate)
        {
            var contextData = KS.GetContextData();
            objectToUpdate.Id = id;
            Func<EventNotificationAction, GenericResponse<EventNotificationAction>> addFunc = (EventNotificationAction coreObject) =>
                EventNotificationActionManager.Instance.Update(contextData, coreObject);
            var response = ClientUtils.GetResponseFromWS(objectToUpdate, addFunc);
            return response;
        }

        /// <summary>
        /// Gets all EventNotification items for a given Object id and type
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.HouseholdRequired)]
        static public KalturaEventNotificationListResponse List(KalturaEventNotificationFilter filter)
        {
            var contextData = KS.GetContextData();
            filter.Validate();
            var coreFilter = AutoMapper.Mapper.Map<EventNotificationActionFilter>(filter);

            Func<GenericListResponse<EventNotificationAction>> listFunc = () =>
                EventNotificationActionManager.Instance.List(contextData, coreFilter);

            KalturaGenericListResponse<KalturaEventNotification> result =
               ClientUtils.GetResponseListFromWS<KalturaEventNotification, EventNotificationAction>(listFunc);

            var response = new KalturaEventNotificationListResponse
            {
                Objects = result.Objects,
                TotalCount = result.TotalCount
            };

            return response;
        }
    }
}