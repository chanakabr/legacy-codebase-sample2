using System;
using ApiObjects;
using ApiObjects.Response;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using ApiLogic.Users.Managers;

namespace WebAPI.Controllers
{
    [Service("campaign")]
    [AddAction(ClientThrows = new eResponseStatus[] { })]
    [UpdateAction(ClientThrows = new eResponseStatus[] { })]
    [DeleteAction(ClientThrows = new eResponseStatus[] { })]
    [ListAction(ClientThrows = new eResponseStatus[] { })]
    public class CampaignController : KalturaCrudController<KalturaCampaign, KalturaCampaignListResponse, Campaign, long, KalturaCampaignFilter>
    {
        /// <summary>
        /// Set campaign's state
        /// </summary>
        /// <param name="campaignId">campaign Id</param>
        /// <param name="newState">new campaign state</param>
        /// <returns>Kaltura campaign object</returns>
        [Action("setState")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        [Throws(eResponseStatus.InternalConnectionIssue)]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static GenericResponse<KalturaCampaign> SetState(long campaignId, KalturaObjectState newState)
        {
            var response = new GenericResponse<KalturaCampaign>();
            var contextData = Managers.Models.KS.GetContextData();

            try
            {
                var _newState = AutoMapper.Mapper.Map<ObjectState>(newState);
                Func<GenericResponse<Campaign>> coreFunc = () =>
                    CampaignManager.Instance.SetState(contextData, campaignId, _newState);

                response.Object = Clients.ClientUtils.GetResponseFromWS<KalturaCampaign, Campaign>(coreFunc);

                if (response.Object != null && response.Status.IsOkStatusCode())
                {
                    response.SetStatus(eResponseStatus.OK);
                }
            }
            catch (Exceptions.ClientException ex)
            {
                Utils.ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}