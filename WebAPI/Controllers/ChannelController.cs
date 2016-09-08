using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/channel/action")]
    [OldStandardAction("addOldStandard", "add")]
    [OldStandardAction("updateOldStandard", "update")]
    public class ChannelController : ApiController
    {
        /// <summary>
        /// Returns channel info        
        /// </summary>
        /// <param name="id">Channel Identifier</param>
        /// <remarks></remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [SchemeArgument("id", MinInteger = 1)]
        public KalturaChannel Get(int id)
        {
            KalturaChannel response = null;

            int groupId = KS.GetFromRequest().GroupId;
            
            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                string language = Utils.Utils.GetLanguageFromRequest();
                response = ClientsManager.CatalogClient().GetChannelInfo(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language, id);

                // if no response - return not found status 
                if (response == null || response.Id == 0)
                {
                    throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Channel");
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
        /// <param name="channelId">channel identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [OldStandard("channelId", "channel_id")]
        [Throws(eResponseStatus.IdentifierRequired)]
        [Throws(eResponseStatus.ObjectNotExist)]
        public bool Delete(int channelId)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().DeleteKSQLChannel(groupId, channelId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new channel for partner. Currently supports only KSQL channel
        /// </summary>
        /// <remarks>
        /// Possible status codes:     
        /// NoObjectToInsert = 4019,
        /// NameRequired = 5005,
        /// </remarks>
        /// <param name="channel">KSQL channel Object</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.NoObjectToInsert)]
        [Throws(eResponseStatus.NameRequired)]
        public KalturaChannel Add(KalturaChannel channel)
        {
            KalturaChannel response = null;

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
        /// Update channel details. Currently supports only KSQL channel
        /// </summary>
        /// <remarks>
        /// Possible status codes:
        /// ObjectNotExist = 4018,
        /// NoObjectToInsert = 4019,
        /// NameRequired = 5005
        /// </remarks>
        /// <param name="channelId">Channel identifier</param>      
        /// <param name="channel">KSQL channel Object</param>       
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.ObjectNotExist)]
        [Throws(eResponseStatus.NoObjectToInsert)]
        [Throws(eResponseStatus.NameRequired)]
        public KalturaChannel Update(int channelId, KalturaChannel channel)
        {
            KalturaChannel response = null;

            int groupId = KS.GetFromRequest().GroupId;
            channel.Id = channelId;

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

        /// <summary>
        /// Insert new channel for partner. Currently supports only KSQL channel
        /// </summary>
        /// <remarks>
        /// Possible status codes:     
        /// NoObjectToInsert = 4019,
        /// NameRequired = 5005,
        /// </remarks>
        /// <param name="channel">KSQL channel Object</param>
        [Route("addOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.NoObjectToInsert)]
        [Throws(eResponseStatus.NameRequired)]
        public KalturaChannelProfile AddOldStandard(KalturaChannelProfile channel)
        {
            KalturaChannelProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().InsertKSQLChannelProfile(groupId, channel);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update channel details. Currently supports only KSQL channel
        /// </summary>
        /// <remarks>
        /// Possible status codes:
        /// ObjectNotExist = 4018,
        /// NoObjectToInsert = 4019,
        /// NameRequired = 5005
        /// </remarks>
        /// <param name="channel">KSQL channel Object</param>       
        [Route("updateOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.ObjectNotExist)]
        [Throws(eResponseStatus.NoObjectToInsert)]
        [Throws(eResponseStatus.NameRequired)]
        public KalturaChannelProfile UpdateOldStandard(KalturaChannelProfile channel)
        {
            KalturaChannelProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().SetKSQLChannelProfile(groupId, channel);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}