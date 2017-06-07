using ApiObjects.Response;
using System.Collections.Generic;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/userInterest/action")]
    public class UserInterestController : ApiController
    {
        /// <summary>
        /// Insert new user interest for partner user
        /// </summary>
        /// <remarks>
        /// Possible status codes:  
        /// PartnerTopicInterestIsMissing, NoUserInterestToInsert, UserInterestAlreadyExist, MetaIdRequired, TopicNotFound , MetaValueRequired 
        /// ParentTopicIsRequired, ParentTopicShouldNotHaveValue, ParentTopicMetaIdNotEqualToMetaParentMetaID, ParentTopicValueIsMissing, ParentIdNotAUserInterest
        /// </remarks>
        /// <param name="userInterest">User interest Object</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.PartnerTopicInterestIsMissing)]
        [Throws(eResponseStatus.NoUserInterestToInsert)]
        [Throws(eResponseStatus.UserInterestAlreadyExist)]
        [Throws(eResponseStatus.MetaIdRequired)]
        [Throws(eResponseStatus.TopicNotFound)]
        [Throws(eResponseStatus.MetaValueRequired)]
        [Throws(eResponseStatus.ParentTopicIsRequired)]
        [Throws(eResponseStatus.ParentTopicShouldNotHaveValue)]
        [Throws(eResponseStatus.ParentTopicMetaIdNotEqualToMetaParentMetaID)]
        [Throws(eResponseStatus.ParentTopicValueIsMissing)]
        [Throws(eResponseStatus.ParentIdNotAUserInterest)]
        public KalturaUserInterest Add(KalturaUserInterest userInterest)
        {
            KalturaUserInterest response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string user = KS.GetFromRequest().UserId;

            try
            {
                // call client
                response = ClientsManager.UsersClient().InsertUserInterest(groupId, user, userInterest);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }


        /// <summary>
        /// Delete new user interest for partner user
        /// </summary>
        /// <remarks>
        /// Possible status codes:             
        /// </remarks>
        /// <param name="id">User interest identifier</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(string id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            string user = KS.GetFromRequest().UserId;

            try
            {
                // call client
                response = ClientsManager.UsersClient().DeleteUserInterest(groupId, user, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns all Engagement for partner
        /// </summary>
        /// <remarks>       
        /// </remarks>
        /// <param name="pager">Page size and index</param>                
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaUserInterestListResponse List()
        {
            List<KalturaUserInterest> list = null;

            int groupId = KS.GetFromRequest().GroupId;
            string user = KS.GetFromRequest().UserId;

            try
            {
                // call client
                list = ClientsManager.UsersClient().GetUserInterests(groupId, user);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            KalturaUserInterestListResponse response = new KalturaUserInterestListResponse()
            {
                UserInterests = list,
                TotalCount = list.Count
            };

            return response;
        }
    }
}