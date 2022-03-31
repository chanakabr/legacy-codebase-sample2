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
    [Service("deviceBrand")]
    public class DeviceBrandController : IKalturaController
    {
        private static readonly IDeviceBrandValidator _deviceBrandValidator = DeviceBrandValidator.Instance;
        private static readonly IDeviceBrandFilterValidator _deviceBrandFilterValidator = DeviceBrandFilterValidator.Instance;

        /// <summary>
        /// Adds a new device brand which belongs to a specific group.
        /// </summary>
        /// <param name="deviceBrand">Device brand.</param>
        /// <returns>Created device brand.</returns>
        /// <remarks>Possible status codes: DeviceFamilyDoesNotExist=5087, DeviceBrandIdAlreadyInUse=5088, ArgumentCannotBeEmpty=50027, ArgumentMaxLengthCrossed=500045, ArgumentNotInPredefinedRange=500092.</remarks>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DeviceFamilyDoesNotExist)]
        [Throws(eResponseStatus.DeviceBrandIdAlreadyInUse)]
        public static KalturaDeviceBrand Add(KalturaDeviceBrand deviceBrand)
        {
            var groupId = Utils.Utils.GetGroupIdFromRequest();
            _deviceBrandValidator.ValidateToAdd(groupId.Value, deviceBrand);

            var userId = Utils.Utils.GetUserIdFromKs();
            var response = ClientsManager.ApiClient().AddDeviceBrand(groupId.Value, deviceBrand, userId);

            return response;
        }

        /// <summary>
        /// Updates an existing device brand which belongs to a specific group.
        /// </summary>
        /// <param name="id">Device brand's identifier.</param>
        /// <param name="deviceBrand">Device brand.</param>
        /// <returns>Updated device brand.</returns>
        /// <remarks>Possible status codes: DeviceFamilyDoesNotExist=5087, DeviceBrandDoesNotExist=5089, ArgumentMaxLengthCrossed=500045, ArgumentNotInPredefinedRange=500092.</remarks>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DeviceFamilyDoesNotExist)]
        [Throws(eResponseStatus.DeviceBrandDoesNotExist)]
        public static KalturaDeviceBrand Update(long id, KalturaDeviceBrand deviceBrand)
        {
            var groupId = Utils.Utils.GetGroupIdFromRequest();
            deviceBrand.Id = id;
            _deviceBrandValidator.ValidateToUpdate(groupId.Value, deviceBrand);

            var userId = Utils.Utils.GetUserIdFromKs();
            var response = ClientsManager.ApiClient().UpdateDeviceBrand(groupId.Value, deviceBrand, userId);

            return response;
        }

        /// <summary>
        /// Return a list of the available device brands.
        /// </summary>
        /// <param name="filter">Filter with no more than one condition specified.</param>
        /// <param name="pager">Page size and index.</param>
        /// <returns>List of <see cref="KalturaDeviceBrand"/> items.</returns>
        /// <remarks>Possible status codes: InvalidArgument=50026.</remarks>
        [Action("list")]
        [ApiAuthorize]
        public static KalturaDeviceBrandListResponse List(KalturaDeviceBrandFilter filter = null, KalturaFilterPager pager = null)
        {
            if (filter == null)
            {
                filter = new KalturaDeviceBrandFilter();
            }

            _deviceBrandFilterValidator.Validate(filter, nameof(filter));

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            var groupId = Utils.Utils.GetGroupIdFromRequest();
            var isSystem = filter.TypeEqual.HasValue
                ? filter.TypeEqual == KalturaDeviceBrandType.System
                : (bool?)null;
            var orderByIdAsc = filter.OrderBy == KalturaDeviceBrandOrderBy.ID_ASC;
            var response = ClientsManager.ApiClient().GetDeviceBrandList(groupId.Value, filter.IdEqual, filter.DeviceFamilyIdEqual, filter.NameEqual, isSystem, orderByIdAsc, pager.GetRealPageIndex(), pager.PageSize.Value);

            return response;
        }
    }
}