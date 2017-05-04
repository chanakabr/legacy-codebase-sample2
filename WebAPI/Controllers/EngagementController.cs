using ApiObjects.Response;
using System.Collections.Generic;
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
    [RoutePrefix("_service/engagement/action")]
    public class EngagementController : ApiController
    {
        /// <summary>
        /// Returns all Engagement for partner
        /// </summary>
        /// <remarks>       
        /// </remarks>
        /// <param name="filter">filter</param>   
        /// <param name="pager">Page size and index</param>                
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaEngagementListResponse List(KalturaEngagementFilter filter)
        {
            List<KalturaEngagement> list = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
                filter = new KalturaEngagementFilter();

            try
            {
                // call client
                list = ClientsManager.NotificationClient().GetEngagements(groupId, filter.getTypeIn(), filter.SendTimeGreaterThanOrEqual);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            KalturaEngagementListResponse response = new KalturaEngagementListResponse()
            {
                Engagements = list,
                TotalCount = list.Count
            };

            return response;
        }

        /// <summary>
        /// Return engagement
        /// </summary>
        /// <param name="id">Engagement identifier</param>
        /// <remarks>       
        /// </remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaEngagement Get(int id)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                return ClientsManager.NotificationClient().GetEngagement(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Delete engagement by engagement adapter id
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// engagement identifier required = 8031, engagement not exist = 8032,  action is not allowed = 5011
        /// </remarks>
        /// <param name="id">Engagement identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.EngagementRequired)]
        [Throws(eResponseStatus.EngagementNotExist)]
        [Throws(eResponseStatus.ActionIsNotAllowed)]
        public bool Delete(int id)
        {
            bool response = false;
            
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().DeleteEngagement(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Insert new Engagement for partner
        /// </summary>
        /// <remarks>
        /// Possible status codes:     
        /// no engagement to insert = 8030
        /// </remarks>
        /// <param name="engagement">Engagement adapter Object</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.NoEngagementToInsert)]
        public KalturaEngagement Add(KalturaEngagement engagement)
        {
            KalturaEngagement response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().InsertEngagement(groupId, engagement);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}