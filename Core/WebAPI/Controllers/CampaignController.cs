using System;
using ApiObjects;
using ApiObjects.Response;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using ApiLogic.Users.Managers;

namespace WebAPI.Controllers
{
    [Service("campaign")]
    [AddAction(ClientThrows = new[] { eResponseStatus.DiscountCodeNotExist, eResponseStatus.NotExist })]
    [UpdateAction(ClientThrows = new[] { eResponseStatus.DiscountCodeNotExist, eResponseStatus.CampaignDoesNotExist, eResponseStatus.NotExist })]
    [DeleteAction(ClientThrows = new[] { eResponseStatus.CampaignDoesNotExist, eResponseStatus.CanDeleteOnlyInactiveCampaign })]
    [ListAction(IsFilterOptional = false, IsPagerOptional = true, ClientThrows = new eResponseStatus[] { })]
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
        [Throws(eResponseStatus.CampaignDoesNotExist)]
        [Throws(eResponseStatus.ExceededMaxCapacity)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static void SetState(long campaignId, KalturaObjectState newState)
        {
            var response = new GenericResponse<KalturaCampaign>();
            response.SetStatus(eResponseStatus.OK);
            var contextData = Managers.Models.KS.GetContextData();

            try
            {
                var _newState = AutoMapper.Mapper.Map<CampaignState>(newState);
                Func<Status> coreFunc = () => CampaignManager.Instance.SetState(contextData, campaignId, _newState);
                Clients.ClientUtils.GetResponseStatusFromWS(coreFunc);
            }
            catch (Exceptions.ClientException ex)
            {
                Utils.ErrorUtils.HandleClientException(ex);
            }
        }
    }
}