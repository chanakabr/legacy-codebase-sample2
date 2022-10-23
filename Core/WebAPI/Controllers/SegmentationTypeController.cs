using ApiObjects;
using ApiObjects.Response;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Segmentation;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("segmentationType")]
    public class SegmentationTypeController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Adds a new segmentation type to the system
        /// </summary>
        /// <param name="segmentationType">The segmentation type to be added</param>
        /// <returns>The segmentation type that was added</returns>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.InvalidParameters)]
        [Throws(eResponseStatus.DynamicSegmentsExceeded)]
        [Throws(eResponseStatus.DynamicSegmentPeriodExceeded)]
        [Throws(eResponseStatus.DynamicSegmentConditionsExceeded)]
        [Throws(eResponseStatus.NameMustBeUnique)]
        static public KalturaSegmentationType Add(KalturaSegmentationType segmentationType)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                long userId = Utils.Utils.GetUserIdFromKs();

                return ClientsManager.ApiClient().AddSegmentationType(groupId, segmentationType, userId);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Updates an existing segmentation type 
        /// </summary>
        /// <param name="segmentationTypeId">The ID of the object that will be updated</param>
        /// <param name="segmentationType">The segmentation type to be updated</param>
        /// <returns>The segmentation type that was updated</returns>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ObjectNotExist)]
        [Throws(eResponseStatus.DynamicSegmentPeriodExceeded)]
        [Throws(eResponseStatus.DynamicSegmentConditionsExceeded)]
        [Throws(eResponseStatus.NameMustBeUnique)]
        static public KalturaSegmentationType Update(long segmentationTypeId, KalturaSegmentationType segmentationType)
        {
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                segmentationType.Id = segmentationTypeId;
                long userId = Utils.Utils.GetUserIdFromKs();

                return ClientsManager.ApiClient().UpdateSegmentationType(groupId, segmentationType, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Delete a segmentation type from the system
        /// </summary>
        /// <param name="id">Segmentation type id</param>
        /// <returns>Whether action succeeded or not</returns>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ObjectNotExist)]
        [Throws(eResponseStatus.CannotDeleteAttachedSegment)]
        static public bool Delete(long id)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                long userId = Utils.Utils.GetUserIdFromKs();

                response = ClientsManager.
                    ApiClient().DeleteSegmentationType(groupId, id, userId);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
        
        /// <summary>
        /// Lists all segmentation types in group
        /// </summary>
        /// <param name="filter">Segmentation type filter - basically empty</param>
        /// <param name="pager">Simple pager</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        static public KalturaSegmentationTypeListResponse List(KalturaBaseSegmentationTypeFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaSegmentationTypeListResponse response = null;

            if (pager == null)
                pager = new KalturaFilterPager();

            if (filter == null)
                filter = new KalturaSegmentationTypeFilter();

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                long userId = Utils.Utils.GetUserIdFromKs();

                switch (filter)
                {
                    case KalturaSegmentationTypeFilter f: response = ListBySegmentationTypeFilter(groupId, userId, pager, f); break;
                    case KalturaSegmentValueFilter f: response = ListBySegmentValueFilter(groupId, userId, pager, f); break;
                    default: throw new NotImplementedException($"List for {filter.objectType} is not implemented");
                }
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        private static KalturaSegmentationTypeListResponse ListBySegmentationTypeFilter(int groupId, long userId, KalturaFilterPager pager, KalturaSegmentationTypeFilter filter)
        {
            HashSet<long> ids = null;
            bool isAllowedToViewInactiveAssets = false;

            if (string.IsNullOrEmpty(filter.Ksql))
            {
                ids = filter.GetIdIn();
            }
            else
            {
                isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(groupId, userId.ToString(), true);
            }

            return ClientsManager.ApiClient().ListSegmentationTypes(groupId, ids, pager.GetRealPageIndex(), pager.PageSize.Value,
                new AssetSearchDefinition() { Filter = filter.Ksql, UserId = userId, IsAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets });
        }

        private static KalturaSegmentationTypeListResponse ListBySegmentValueFilter(int groupId, long userId, KalturaFilterPager pager, KalturaSegmentValueFilter filter)
        {
            bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(groupId, userId.ToString(), true);

            return ClientsManager.ApiClient().GetSegmentationTypesBySegmentIds(groupId, filter.GetIdIn(),
                new AssetSearchDefinition() { UserId = userId, IsAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets },
                pager.GetRealPageIndex(), pager.PageSize.Value);
        }

        /// <summary>
        /// Gets existing partner segmentation configuration
        /// </summary>
        /// <returns>partner segmentation configuration</returns>
        [Action("getPartnerConfiguration")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static KalturaSegmentationPartnerConfiguration GetPartnerConfiguration()
        {
            throw new ApiException() { Code = 1, Message = "SegmentationTypeController.GetPartnerConfiguration is not implemented in phoenix" };
        }

        /// <summary>
        /// Sets partner configuration for segments configuration
        /// </summary>
        /// <param name="configuration">1. maxDynamicSegments - how many dynamic segments (segments with conditions) the operator is allowed to have.
        /// Displayed in the OPC as *'Maximum Number of Dynamic Segments' 
        /// *maxCalculatedPeriod - 
        /// the maximum number of past days to be calculated for dynamic segments. e.g. the last 60 days, the last 90 days etc.
        /// Displayed in OPC as *'Maximum of Dynamic Segments period'*</param>
        /// <returns></returns>
        [Action("updatePartnerConfiguration")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static bool UpdatePartnerConfiguration(KalturaSegmentationPartnerConfiguration configuration)
        {
            throw new ApiException() { Code = 1, Message = "SegmentationTypeController.UpdatePartnerConfiguration is not implemented in phoenix" };
        }
    }
}