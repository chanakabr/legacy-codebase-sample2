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
        /// <param name="channel_id">Channel Identifier</param>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="language">Language Code</param>
        /// <param name="household_id">Household Identifier</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize(true)]
        public KalturaChannel Get(string partner_id, int channel_id, string language = null, int household_id = 0)
        {
            KalturaChannel response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (channel_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "channel_id cannot be 0");
            }

            try
            {
                response = ClientsManager.CatalogClient().GetChannelInfo(groupId, KS.GetFromRequest().UserId, household_id, language, channel_id);

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