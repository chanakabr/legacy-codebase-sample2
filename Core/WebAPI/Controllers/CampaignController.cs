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
        /// Activate campaign
        /// </summary>
        /// <param name="campaignId">campaign Id</param>
        /// <returns>Kaltura campaign object</returns>
        [Action("activate")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        [Throws(eResponseStatus.InternalConnectionIssue)]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static GenericResponse<KalturaCampaign> Activate(long campaignId)
        {
            var response = new GenericResponse<KalturaCampaign>();
            var contextData = Managers.Models.KS.GetContextData();

            try
            {
                Func<GenericResponse<Campaign>> coreFunc = () =>
                    CampaignManager.Instance.ActivateTriggerCampaign(contextData, campaignId);

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

        /// <summary>
        /// Deactivate campaign
        /// </summary>
        /// <param name="campaignId">campaign Id</param>
        /// <returns>Kaltura campaign object</returns>
        [Action("deactivate")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        [Throws(eResponseStatus.InternalConnectionIssue)]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static GenericResponse<KalturaCampaign> Deactivate(long campaignId)
        {
            var response = new GenericResponse<KalturaCampaign>();
            var contextData = Managers.Models.KS.GetContextData();

            try
            {
                Func<GenericResponse<Campaign>> coreFunc = () =>
                    CampaignManager.Instance.DeactivateTriggerCampaign(contextData, campaignId);

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