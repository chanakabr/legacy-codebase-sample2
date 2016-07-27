using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.API;
using WebAPI.Models.Billing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/externalChannelProfile/action")]
    [OldStandardAction("listOldStandard", "list")]
    [OldStandardAction("updateOldStandard", "update")]
    public class ExternalChannelProfileController : ApiController
    {
        /// <summary>
        /// Returns all External channels for partner 
        /// </summary>
        /// <remarks>       
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaExternalChannelProfileListResponse List()
        {
            List<KalturaExternalChannelProfile> list = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                list = ClientsManager.ApiClient().GetExternalChannels(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaExternalChannelProfileListResponse() { Objects = list, TotalCount = list.Count };
        }

        /// <summary>
        /// Returns all External channels for partner 
        /// </summary>
        /// <remarks>       
        /// </remarks>
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public List<KalturaExternalChannelProfile> ListOldStandard()
        {
            List<KalturaExternalChannelProfile> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetExternalChannels(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete External channel by External channel id
        /// </summary>
        /// <remarks>
        /// Possible status codes:   
        /// external channel not exist = 4011, external channel identifier required = 4013
        /// </remarks>
        /// <param name="externalChannelId">External channel identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [OldStandard("externalChannelId", "external_channel_id")]
        public bool Delete(int externalChannelId)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().DeleteExternalChannel(groupId, externalChannelId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new External channel for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:     
        /// recommendation engine not exist = 4007, recommendation engine identifier required = 4008, inactive external channel enrichment = 4016, 
        /// name required = 5005, external identifier required = 6016, external identifier must be unique = 6040
        /// </remarks>
        /// <param name="externalChannel">External channel Object</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [OldStandard("externalChannel", "external_channel")]
        public KalturaExternalChannelProfile Add(KalturaExternalChannelProfile externalChannel)
        {
            KalturaExternalChannelProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().InsertExternalChannel(groupId, externalChannel);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update External channel details
        /// </summary>
        /// <remarks>
        /// Possible status codes:   
        /// external channel not exist = 4011, external channel identifier required = 4013, inactive external channel enrichment = 4016,
        /// name required = 5005, external identifier required = 6016, external identifier must be unique = 6040  
        /// </remarks>
        /// <param name="externalChannelId">External channel identifier</param>       
        /// <param name="externalChannel">External channel Object</param>       
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public KalturaExternalChannelProfile Update(int externalChannelId, KalturaExternalChannelProfile externalChannel)
        {
            KalturaExternalChannelProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;
            externalChannel.Id = externalChannelId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().SetExternalChannel(groupId, externalChannel);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update External channel details
        /// </summary>
        /// <remarks>
        /// Possible status codes:   
        /// external channel not exist = 4011, external channel identifier required = 4013, inactive external channel enrichment = 4016,
        /// name required = 5005, external identifier required = 6016, external identifier must be unique = 6040  
        /// </remarks>
        /// <param name="external_channel">External channel Object</param>       
        [Route("updateOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaExternalChannelProfile UpdateOldStandard(KalturaExternalChannelProfile external_channel)
        {
            KalturaExternalChannelProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().SetExternalChannel(groupId, external_channel);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}