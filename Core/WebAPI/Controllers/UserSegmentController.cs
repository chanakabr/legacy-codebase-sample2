using ApiObjects;
using ApiObjects.Response;
using System;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Segmentation;
using WebAPI.Utils;

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
        static public KalturaUserSegmentListResponse List(KalturaUserSegmentFilter filter, KalturaFilterPager pager = null)
        {
            KalturaUserSegmentListResponse response = null;
            bool isAllowedToViewInactiveAssets = false;

            int groupId = KS.GetFromRequest().GroupId;

            if (pager == null)
                pager = new KalturaFilterPager();

            string userId = string.Empty;

            if (!string.IsNullOrEmpty(filter.UserIdEqual))
            {
                userId = filter.UserIdEqual;
            }
            else
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "filter..userIdEqual");
            }


            if (!string.IsNullOrEmpty(filter.Ksql))                
            {
                isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(groupId, userId.ToString(), true);
            }

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetUserSegments(groupId, userId,
                    new AssetSearchDefinition() { Filter = filter.Ksql, UserId = long.Parse(userId), IsAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets }, pager.getPageIndex(), pager.getPageSize());
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
        [Throws(eResponseStatus.InvalidUser)]
        [Throws(eResponseStatus.ObjectNotExist)]
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
        /// <returns></returns>
        [Action("delete")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.ObjectNotExist)]
        static public bool Delete(string userId, long segmentId)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;

                response = ClientsManager.
                    ApiClient().DeleteUserSegment(groupId, userId, segmentId);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}