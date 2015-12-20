using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.Billing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/ksqlChannelProfile/action")]
    public class KSQLChannelProfileController : ApiController
    {
        /// <summary>
        /// Returns a KSQL channel by its ID
        /// </summary>
        /// <remarks>       
        /// </remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaKSQLChannelProfile Get(int channel_id)
        {
            KalturaKSQLChannelProfile response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetKSQLChannel(groupId, channel_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete KSQL channel by its channel id
        /// </summary>
        /// <remarks>
        /// Possible status codes:   
        /// 
        /// 
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
        /// 
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
        /// 
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