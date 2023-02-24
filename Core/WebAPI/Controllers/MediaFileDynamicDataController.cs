using System.Linq;
using ApiLogic.Catalog.CatalogManagement.Managers;
using ApiObjects.Response;
using ApiObjects.User;
using WebAPI.Clients;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Mappers;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.ModelsValidators;

namespace WebAPI.Controllers
{
    [Service("mediaFileDynamicData")]
    public class MediaFileDynamicDataController : IKalturaController
    {
        private static readonly IMediaFileTypeDynamicDataManager _mediaFileTypeDynamicDataManager =
            MediaFileTypeDynamicDataManager.Instance;

        /// <summary>
        /// Add a dynamicData value to the values list of a specific key name in a specific mediaFileTypeId
        /// </summary>
        /// <param name="dynamicData">DynamicData value</param>
        /// <returns></returns>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DynamicDataKeyDoesNotExist)]
        public static KalturaMediaFileDynamicData Add(KalturaMediaFileDynamicData dynamicData)
        {
            var ks = KS.GetFromRequest();
            var groupId = ks.GroupId;
            var userId = ks.UserId.ParseUserId();

            var mappedData = MediaFileDynamicDataMapper.Map(dynamicData);

            var response = _mediaFileTypeDynamicDataManager.AddMediaFileDynamicDataValue(groupId, mappedData, userId);

            return MediaFileDynamicDataMapper.Map(response.Get());
        }

        /// <summary>
        /// List and filter existing mediaFile dynamicData values
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="pager">Pager</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        public static KalturaMediaFileDynamicDataListResponse List(KalturaMediaFileDynamicDataFilter filter,
            KalturaFilterPager pager = null)
        {
            var result = new KalturaMediaFileDynamicDataListResponse();

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            filter.Validate(nameof(filter));

            var ks = KS.GetFromRequest();
            var groupId = ks.GroupId;
            var idIn = Utils.Utils.ParseCommaSeparatedValues<long>(filter.IdIn, $"{nameof(filter)}.idIn",
                checkDuplicate: true, ignoreDefaultValueValidation: true).ToList();

            var response = _mediaFileTypeDynamicDataManager.GetMediaFileDynamicDataValues(groupId, idIn, filter.MediaFileTypeId,
                filter.MediaFileTypeKeyName, filter.ValueEqual, filter.ValueStartsWith, pager.GetPageIndex(),
                pager.PageSize.Value);

            var (items, totalCount) = response.GetList();
            result.Objects = MediaFileDynamicDataMapper.Map(items);
            result.TotalCount = totalCount;
            return result;
        }

        /// <summary>
        /// Delete an existing DynamicData value
        /// </summary>
        /// <param name="id">DynamicData identifier</param>
        /// <returns></returns>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DynamicDataKeyValueDoesNotExist)]
        public static bool Delete(long id)
        {
            var ks = KS.GetFromRequest();
            var groupId = ks.GroupId;
            var userId = ks.UserId.ParseUserId();

            var response = _mediaFileTypeDynamicDataManager.DeleteMediaFileDynamicDataValue(groupId, id, userId);

            return response.Get();
        }
    }
}
