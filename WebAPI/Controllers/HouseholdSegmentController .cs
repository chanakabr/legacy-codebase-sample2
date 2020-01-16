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
    /// <summary>
    /// Household Segment
    /// </summary>
    [Service("householdSegment")]
    public class HouseholdSegmentController : IKalturaController
    {
        /// <summary>
        /// Retrieve all the segments that apply for given household
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

        static public KalturaHouseholdSegmentListResponse List()
        {
            KalturaHouseholdSegmentListResponse response = null;

            try
            {
                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Adds a segment to a household
        /// </summary>
        /// <param name="householdSegment">Household segment</param>
        /// <returns></returns>
        [Action("add")]
        [ApiAuthorize]
        static public KalturaHouseholdSegment Add(KalturaHouseholdSegment householdSegment)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                return ClientsManager.ApiClient().AddHouseholdSegment(groupId, householdSegment);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Deletes a segment from a household
        /// </summary>
        /// <param name="householdId">Household id</param>
        /// <param name="segmentId">Segemnt id</param>
        /// <returns></returns>
        [Action("delete")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        static public bool Delete(long householdId, long segmentId)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;

                response = ClientsManager.ApiClient().DeleteHouseholdSegment(groupId, householdId, segmentId);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}