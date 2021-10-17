using ApiLogic.Users.Managers;
using ApiObjects.Response;
using ApiObjects.User.SessionProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Clients;
using WebAPI.Managers.Models;
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
            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                filter = new KalturaUserSessionProfileFilter();
            }

            Func<IReadOnlyCollection<UserSessionProfile>> getListFunc = () =>
               UserSessionProfileManager.Instance.List(groupId, filter.IdEqual);

            KalturaGenericListResponse<KalturaUserSessionProfile> response =
                ClientUtils.ListFromLogic<KalturaUserSessionProfile, UserSessionProfile>(getListFunc);

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            var result = new KalturaUserSessionProfileListResponse()
            {
                Objects = response.Objects.OrderBy(x => x.Id).Skip(pager.PageSize.Value * pager.PageIndex.Value).Take(pager.PageSize.Value).ToList(),
                TotalCount = response.TotalCount
            };

            return result;
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
            userSessionProfile.ValidateForAdd();
            var contextData = KS.GetContextData();

            Func<UserSessionProfile, GenericResponse<UserSessionProfile>> addFunc = (UserSessionProfile objectToAdd) =>
                        UserSessionProfileManager.Instance.Add(contextData, objectToAdd);

            var result = ClientUtils.GetResponseFromWS(userSessionProfile, addFunc);

            return result;
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
            userSessionProfile.ValidateForUpdate();
            var contextData = KS.GetContextData();
            userSessionProfile.Id = id;

            Func<UserSessionProfile, GenericResponse<UserSessionProfile>> updateFunc = (UserSessionProfile objectToUpdate) =>
                        UserSessionProfileManager.Instance.Update(contextData, objectToUpdate);

            var result = ClientUtils.GetResponseFromWS(userSessionProfile, updateFunc);

            return result;
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
            var contextData = KS.GetContextData();
            Func<Status> deleteFunc = () => UserSessionProfileManager.Instance.Delete(contextData, id);
            ClientUtils.GetResponseStatusFromWS(deleteFunc);
        }
    }
}