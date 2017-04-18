using System;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/topic/action")]
    public class TopicController : ApiController
    {

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// AnnouncementNotFound = 8006
        /// </remarks>
        /// <param name="id">Topic identifier</param>        
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [SchemeArgument("id", MinInteger = 1)]
        public KalturaTopic Get(int id)
        {
            KalturaTopic response = null;

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
        /// Get list of topics
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// </remarks>
        /// <param name="filter">Topics filter</param>     
        /// <param name="pager">Page size and index</param>        
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaTopicListResponse List(KalturaTopicFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaTopicListResponse response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;

                if (pager == null)
                    pager = new KalturaFilterPager();

                // call client                
                response = ClientsManager.NotificationClient().GetTopicsList(groupId, pager.getPageSize(), pager.getPageIndex());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// </remarks>
        /// <param name="pager">Page size and index</param>        
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [OldStandardAction("list")]
        [Obsolete]
        public KalturaTopicResponse ListOldStandard(KalturaFilterPager pager = null)
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
        /// Deleted a topic
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// AnnouncementNotFound = 8006
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
        /// Updates a topic "automatic issue notification" behavior.
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// 
        /// </remarks>
        /// <param name="id">Topic identifier</param>        
        /// <param name="automaticIssueNotification">Behavior options:
        ///  Inherit = 0: Take value from partner notification settings
        ///  Yes = 1: Issue a notification massage when a new episode is available on the catalog
        ///  No = 2: Do send a notification message when a new episode is available on the catalog
        /// </param>        
        [Route("updateStatus"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
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