using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/TimeShiftedTvPartnerSettings/action")]
    public class TimeShiftedTvPartnerSettingsController : ApiController
    {
        /// <summary>
        /// Retrieve the partner's time shifted tv settings.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>The time shifted tv settings that apply for the partner</returns>
        /// 
        [Route("get"), HttpPost]
        [ApiAuthorize]
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
        /// Update the partner's time shifted tv settings.      
        /// </summary>    
        /// 
        /// <remarks>        
        /// </remarks>
        /// <returns>The time shifted tv settings that apply for the partner</returns>
        /// 
        [Route("update"), HttpPost]
        [ApiAuthorize]
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