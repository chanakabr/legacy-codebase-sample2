using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
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
        [ApiAuthorize]
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


        /// <summary>
        /// Delete channel by its channel id
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// IdentifierRequired = 4017,
        /// ObjectNotExist = 4018
        /// </remarks>
        /// <param name="channel_id">channel identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(int channel_id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().DeleteKSQLChannel(groupId, channel_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new KSQL channel for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:     
        /// NoObjectToInsert = 4019,
        /// NameRequired = 5005,
        /// </remarks>
        /// <param name="channel">KSQL channel Object</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaKSQLChannelProfile Add(KalturaKSQLChannelProfile channel)
        {
            KalturaKSQLChannelProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().InsertKSQLChannel(groupId, channel);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update KSQL channel details
        /// </summary>
        /// <remarks>
        /// Possible status codes:
        /// ObjectNotExist = 4018,
        /// NoObjectToInsert = 4019,
        /// NameRequired = 5005
        /// </remarks>
        /// <param name="channel">KSQL channel Object</param>       
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public KalturaKSQLChannelProfile Update(KalturaKSQLChannelProfile channel)
        {
            KalturaKSQLChannelProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().SetKSQLChannel(groupId, channel);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}