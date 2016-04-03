using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/LinearChannel/action")]
    public class LinearChannelController : ApiController
    {
        /// <summary>
        /// update epg channel.
        /// </summary>    
        /// 
        /// <remarks>        
        /// </remarks>
        /// <returns>Status</returns>
        /// 
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(int epg_channel_id, bool cdvr = false, bool catch_up = false, int catch_up_buffer = 0, bool start_over = false, bool live_trick_play = false, int live_trick_play_buffer = 0)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
               // response = ClientsManager.NotificationClient().Update(groupId, userId, settings);
                response = true;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}