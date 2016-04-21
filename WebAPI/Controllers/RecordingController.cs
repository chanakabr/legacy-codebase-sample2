using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/Recording/action")]
    public class RecordingController : ApiController
    {
        /// <summary>
        /// Query record options for a program
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>The recording availability and status for for the requested program</returns>
        /// 
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaRecording Get(long epg_id)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.ConditionalAccessClient().QueryRecord(groupId, userId, epg_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Issue a record request for a program
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns>The recording ID and status for options for the requested program</returns>
        /// 
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaRecording Add(long epg_id)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.ConditionalAccessClient().Record(groupId, userId, epg_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}