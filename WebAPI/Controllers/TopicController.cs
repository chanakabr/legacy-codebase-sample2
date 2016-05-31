using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/topic/action")]
    public class TopicController : ApiController
    {

        /// <summary>
        /// TBD
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// 
        /// </remarks>
        /// <param name="id">topic id</param>        
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaTopic Get(int id)
        {
            KalturaTopic response = null;

            // parameters validation
            if (id <= 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Topic id is illegal");
            }

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                // call client                
                response = ClientsManager.NotificationClient().GetTopic(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// </remarks>
        /// <param name="pager">Page size and index</param>        
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaTopicResponse List(KalturaFilterPager pager = null)
        {
            KalturaTopicResponse response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                if (pager == null)
                    pager = new KalturaFilterPager();

                // call client                
                response = ClientsManager.NotificationClient().GetTopics(groupId, pager.getPageSize(), pager.getPageIndex());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// TBD
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// 
        /// </remarks>
        /// <param name="id">Topic identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(int id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().DeleteTopic(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }


        /// <summary>
        /// TBD
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// 
        /// </remarks>
        /// <param name="id"></param>        
        /// <param name="automaticIssueNotification"></param>        
        [Route("updateStatus"), HttpPost]
        [ApiAuthorize]
        public bool UpdateStatus(int id, KalturaTopicAutomaticIssueNotification automaticIssueNotification)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                // call client                
                response = ClientsManager.NotificationClient().UpdateTopic(groupId, id, automaticIssueNotification);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}