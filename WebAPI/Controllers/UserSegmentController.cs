using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.ObjectsConvertor.Mapping.Utils;
using WebAPI.Utils;
using WebAPI.Models.Segmentation;
using WebAPI.Models.General;

namespace WebAPI.Controllers
{
    [Service("userSegment")]
    public class UserSegment : IKalturaController
    {
        /// <summary>
        /// Retrieve all the segments that apply for given user
        /// </summary>
        /// <remarks>Possible status codes: 
        /// User does not exist = 2000, User with no household = 2024, User suspended = 2001, User not in household = 1005, Household does not exist = 1006</remarks>
        /// <param name="filter">Filter</param>
        /// <param name="pager">Pager</param>
        /// <returns>All the segments that apply for user in filter</returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.DomainNotExists)]

        static public KalturaUserSegmentListResponse List(KalturaUserSegmentFilter filter, KalturaFilterPager pager = null)
        {
            KalturaUserSegmentListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (pager == null)
                pager = new KalturaFilterPager();

            string userId = string.Empty;

            if (!string.IsNullOrEmpty(filter.UserIdEqual))
            {
                userId = filter.UserIdEqual;
            }

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetUserSegments(groupId, userId, pager.getPageIndex(), pager.getPageSize());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Adds a segment to a user
        /// </summary>
        /// <param name="userSegment">User segment</param>
        /// <param name="userId">User Identifier</param>
        /// <returns></returns>
        [Action("add")]
        [ApiAuthorize]
        static public KalturaUserSegment Add(KalturaUserSegment userSegment)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                return ClientsManager.ApiClient().AddUserSegment(groupId, userSegment);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Deletes a segment from a user
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="segmentId">Segment id</param>
        /// <param name="segmentationTypeId">Segmentation type id</param>
        /// <returns></returns>
        [Action("delete")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        static public bool Delete(string userId, long segmentationTypeId, long? segmentId)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;

                response = ClientsManager.
                    ApiClient().DeleteUserSegment(groupId, userId, segmentationTypeId, segmentId);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}