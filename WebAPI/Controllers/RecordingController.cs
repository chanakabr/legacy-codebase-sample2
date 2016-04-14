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
        /// <returns>The recording options for The program time shifted tv settings that apply for the partner</returns>
        /// 
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public List<KalturaRecording> List(long[] epgIDs)
        {
            List<KalturaRecording> response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.ConditionalAccessClient().QueryRecords(groupId, userId, epgIDs);
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
        /// <returns>The recording options for The program time shifted tv settings that apply for the partner</returns>
        /// 
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaRecording Add(int epgID)
        {
            KalturaRecording response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.ConditionalAccessClient().Record(groupId, userId, epgID);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}