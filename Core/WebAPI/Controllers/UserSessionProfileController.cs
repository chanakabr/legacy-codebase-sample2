using System;
using ApiObjects.Response;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Users.UserSessionProfile;

namespace WebAPI.Controllers
{
    [Service("userSessionProfile")]
    public class UserSessionProfileController : IKalturaController
    {
        /// <summary>
        /// Returns the list of available UserSessionProfiles
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="pager">Pager</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaUserSessionProfileListResponse List(KalturaUserSessionProfileFilter filter = null, KalturaFilterPager pager = null)
        {
            throw new NotImplementedException("call should go to rest-proxy service instead of Phoenix");
        }

        /// <summary>
        /// Add new UserSessionProfile
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="userSessionProfile">userSessionProfile Object to add</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ExceededMaxCapacity)]
        [Throws(eResponseStatus.DeviceBrandIdsDoesNotExist)]
        [Throws(eResponseStatus.NonExistingDeviceFamilyIds)]
        [Throws(eResponseStatus.SegmentsIdsDoesNotExist)]
        [Throws(eResponseStatus.DeviceManufacturerIdsDoesNotExist)]
        static public KalturaUserSessionProfile Add(KalturaUserSessionProfile userSessionProfile)
        {
            throw new NotImplementedException("call should go to rest-proxy service instead of Phoenix");
        }

        /// <summary>
        /// Update existing UserSessionProfile
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">id of userSessionProfile to update</param>
        /// <param name="userSessionProfile">userSessionProfile Object to update</param>
        [Action("update")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.UserSessionProfileDoesNotExist)]
        [Throws(eResponseStatus.DeviceBrandIdsDoesNotExist)]
        [Throws(eResponseStatus.NonExistingDeviceFamilyIds)]
        [Throws(eResponseStatus.SegmentsIdsDoesNotExist)]
        [Throws(eResponseStatus.DeviceManufacturerIdsDoesNotExist)]
        static public KalturaUserSessionProfile Update(long id, KalturaUserSessionProfile userSessionProfile)
        {
            throw new NotImplementedException("call should go to rest-proxy service instead of Phoenix");
        }

        /// <summary>
        /// Delete existing UserSessionProfile
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">UserSessionProfile identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.UserSessionProfileDoesNotExist)]
        static public void Delete(long id)
        {
            throw new NotImplementedException("call should go to rest-proxy service instead of Phoenix");
        }
    }
}