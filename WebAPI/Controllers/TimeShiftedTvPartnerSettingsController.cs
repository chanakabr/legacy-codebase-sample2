using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/timeShiftedTvPartnerSettings/action")]
    public class TimeShiftedTvPartnerSettingsController : ApiController
    {

        /// <summary>
        /// Retrieve the account’s time-shifted TV settings (catch-up and C-DVR, Trick-play, Start-over)
        /// </summary>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, TimeShiftedTvPartnerSettingsNotFound = 5022</remarks>   
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.TimeShiftedTvPartnerSettingsNotFound)]
        public KalturaTimeShiftedTvPartnerSettings Get()
        {
            KalturaTimeShiftedTvPartnerSettings response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.ApiClient().GetTimeShiftedTvPartnerSettings(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Configure the account’s time-shifted TV settings (catch-up and C-DVR, Trick-play, Start-over)
        /// </summary>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, TimeShiftedTvPartnerSettingsNotSent = 5023, TimeShiftedTvPartnerSettingsNegativeBufferSent = 5024</remarks>  
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        [Throws(eResponseStatus.TimeShiftedTvPartnerSettingsNotSent)]
        [Throws(eResponseStatus.TimeShiftedTvPartnerSettingsNegativeBufferSent)]
        public bool Update(KalturaTimeShiftedTvPartnerSettings settings)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                // call client
                response = ClientsManager.ApiClient().UpdateTimeShiftedTvPartnerSettings(groupId, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

    }
}