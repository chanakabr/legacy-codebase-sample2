using ApiObjects;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Segmentation;
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
        static public KalturaSegmentationTypeListResponse List(KalturaSegmentationTypeFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaSegmentationTypeListResponse response = null;
            bool isFilterValid = false;

            if (pager == null)
                pager = new KalturaFilterPager();

            if (filter == null)
            {
                filter = new KalturaSegmentationTypeFilter();
                isFilterValid = true;
            }
            else
            {
                isFilterValid = filter.Validate();
            }

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                long userId = Utils.Utils.GetUserIdFromKs();

                List<long> ids = null;
                bool isAllowedToViewInactiveAssets = false;

                if (string.IsNullOrEmpty(filter.Ksql))
                {
                    ids = filter.GetIdIn();
                }
                else
                {
                    isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(groupId, userId.ToString(), true);
                }

                

                response = ClientsManager.ApiClient().ListSegmentationTypes(groupId, ids, pager.getPageIndex(), pager.getPageSize(), 
                    new AssetSearchDefinition() { Filter = filter.Ksql, UserId = userId, IsAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets });                
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
        
    }
}