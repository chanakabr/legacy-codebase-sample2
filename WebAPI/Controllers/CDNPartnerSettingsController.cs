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
    [RoutePrefix("_service/cDNPartnerSettings/action")]
    public class CDNPartnerSettingsController : ApiController
    {

        /// <summary>
        /// Retrieve the partner’s CDN settings (default adapters)
        /// </summary>
        /// <returns></returns>
        /// <remarks>Possible status codes: CDN partner settings not found = 5025</remarks>   
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaCDNPartnerSettings Get()
        {
            KalturaCDNPartnerSettings response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                // call client                
                response = ClientsManager.ApiClient().GetCDNPartnerSettings(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Configure the partner’s CDN settings (default adapters)
        /// </summary>
        /// <returns></returns>
        /// <remarks>Possible status codes: CDN partner settings not found = 5025</remarks>   
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public KalturaCDNPartnerSettings Update(KalturaCDNPartnerSettings settings)
        {
            KalturaCDNPartnerSettings response = null;

            if (settings != null &&
                ((settings.DefaultVodAdapter.HasValue && settings.DefaultVodAdapter < 0) || 
                (settings.DefaultEpgAdapter.HasValue && settings.DefaultEpgAdapter < 0) || 
                (settings.DefaultRecordingAdapter.HasValue && settings.DefaultRecordingAdapter < 0)))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "default adapters' IDs cannot be negative");
            }

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                // call client                
                response = ClientsManager.ApiClient().UpdateCDNSettings(groupId, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

    }
}