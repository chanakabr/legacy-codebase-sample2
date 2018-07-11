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
    [Service("cdnPartnerSettings")]
    public class CdnPartnerSettingsController : IKalturaController
    {

        /// <summary>
        /// Retrieve the partner’s CDN settings (default adapters)
        /// </summary>
        /// <returns></returns>
        /// <remarks>Possible status codes: CDN partner settings not found = 5025</remarks>   
        [Action("get")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.CDNPartnerSettingsNotFound)]
        static public KalturaCDNPartnerSettings Get()
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
        /// <param name="settings">CDN partner settings</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: CDN partner settings not found = 5025</remarks>   
        [Action("update")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.CDNPartnerSettingsNotFound)]
        static public KalturaCDNPartnerSettings Update(KalturaCDNPartnerSettings settings)
        {
            KalturaCDNPartnerSettings response = null;
            
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