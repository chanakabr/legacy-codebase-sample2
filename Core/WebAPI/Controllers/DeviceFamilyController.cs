using ApiObjects.Response;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.Validation;

namespace WebAPI.Controllers
{
    [Service("deviceFamily")]
    public class DeviceFamilyController : IKalturaController
    {
        private static readonly IDeviceFamilyValidator _deviceFamilyValidator = DeviceFamilyValidator.Instance;
        private static readonly IDeviceFamilyFilterValidator _deviceFamilyFilterValidator = DeviceFamilyFilterValidator.Instance;

        /// <summary>
        /// Adds a new device family which belongs to a specific group.
        /// </summary>
        /// <param name="deviceFamily">Device family.</param>
        /// <returns>Created device family.</returns>
        /// <remarks>Possible status codes: DeviceFamilyIdAlreadyInUse=5086, ArgumentCannotBeEmpty=50027, ArgumentMaxLengthCrossed=500045, ArgumentNotInPredefinedRange=500092.</remarks>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DeviceFamilyIdAlreadyInUse)]
        public static KalturaDeviceFamily Add(KalturaDeviceFamily deviceFamily)
        {
            var groupId = Utils.Utils.GetGroupIdFromRequest();
            _deviceFamilyValidator.ValidateToAdd(groupId.Value, deviceFamily);

            var userId = Utils.Utils.GetUserIdFromKs();
            var response = ClientsManager.ApiClient().AddDeviceFamily(groupId.Value, deviceFamily, userId);

            return response;
        }

        /// <summary>
        /// Updates an existing device family which belongs to a specific group.
        /// </summary>
        /// <param name="id">Device family's identifier.</param>
        /// <param name="deviceFamily">Device family.</param>
        /// <returns>Updated device family.</returns>
        /// <remarks>Possible status codes: DeviceFamilyDoesNotExist=5087, ArgumentMaxLengthCrossed=500045, ArgumentNotInPredefinedRange=500092DeviceFamilyDoesNotExist=5087, ArgumentMaxLengthCrossed=500045, ArgumentNotInPredefinedRange=500092.</remarks>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DeviceFamilyDoesNotExist)]
        public static KalturaDeviceFamily Update(long id, KalturaDeviceFamily deviceFamily)
        {
            var groupId = Utils.Utils.GetGroupIdFromRequest();
            deviceFamily.Id = id;
            _deviceFamilyValidator.ValidateToUpdate(groupId.Value, deviceFamily);

            var userId = Utils.Utils.GetUserIdFromKs();
            var response = ClientsManager.ApiClient().UpdateDeviceFamily(groupId.Value, deviceFamily, userId);

            return response;
        }

        /// <summary>
        /// Return a list of the available device families.
        /// </summary>
        /// <param name="filter">Filter with no more than one condition specified.</param>
        /// <param name="pager">Page size and index.</param>
        /// <returns>List of <see cref="KalturaDeviceFamily"/> items.</returns>
        /// <remarks>Possible status codes: InvalidArgument=50026.</remarks>
        [Action("list")]
        [ApiAuthorize]
        public static KalturaDeviceFamilyListResponse List(KalturaDeviceFamilyFilter filter = null, KalturaFilterPager pager = null)
        {
            if (filter == null)
            {
                filter = new KalturaDeviceFamilyFilter();
            }

            _deviceFamilyFilterValidator.Validate(filter, nameof(filter));

            if (pager == null)
            {
                pager = new KalturaFilterPager { PageSize = int.MaxValue };
            }

            var groupId = Utils.Utils.GetGroupIdFromRequest();
            var isSystem = filter.TypeEqual.HasValue
                ? filter.TypeEqual == KalturaDeviceFamilyType.System
                : (bool?)null;
            var orderByIdAsc = filter.OrderBy == KalturaDeviceFamilyOrderBy.ID_ASC;
            var response = ClientsManager.ApiClient().GetDeviceFamilies(groupId.Value, filter.IdEqual, filter.NameEqual, isSystem, orderByIdAsc, pager.GetRealPageIndex(), pager.PageSize.Value);

            return response;
        }
    }
}