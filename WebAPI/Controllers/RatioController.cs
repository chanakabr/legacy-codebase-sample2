using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/ratio/action")]
    public class RatioController : ApiController
    {
        /// <summary>
        /// Get the list of available ratios
        /// </summary>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaRatioListResponse List()
        {
            KalturaRatioListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.CatalogClient().GetRatios(groupId);
              
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Add new group ratio
        /// </summary>
        /// <param name="ratio">Ratio to add for the partner</param>
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaRatio Add(KalturaRatio ratio)
        {
            KalturaRatio response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            if (string.IsNullOrEmpty(ratio.Name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }

            ratio.Validate();

            try
            {
                response = ClientsManager.CatalogClient().AddRatio(groupId, userId, ratio);

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}