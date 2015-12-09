using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/channel/action")]
    public class ChannelController : ApiController
    {
        /// <summary>
        /// Returns channel info        
        /// </summary>
        /// <param name="id">Channel Identifier</param>
        /// <param name="language">Language Code</param>        
        /// <remarks></remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize(true)]
        public KalturaChannel Get(int id, string language = null)
        {
            KalturaChannel response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (id <= 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal channel ID");
            }

            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;

                response = ClientsManager.CatalogClient().GetChannelInfo(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language, id);

                // if no response - return not found status 
                if (response == null || response.Id == 0)
                {
                    throw new NotFoundException();
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}