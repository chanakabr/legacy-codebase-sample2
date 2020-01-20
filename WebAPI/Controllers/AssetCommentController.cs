using KLogMonitor;
using System;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("assetComment")]
    class AssetCommentController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        /// <summary>
        /// Returns asset comments by asset id
        /// </summary>
        /// <param name="filter">Filtering the assets comments request</param>
        /// <param name="pager">Page size and index</param>
        /// <remarks></remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaAssetCommentListResponse List(KalturaAssetCommentFilter filter, KalturaFilterPager pager = null)
        {
            KalturaAssetCommentListResponse response = null;
           
            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                var ks = KSManager.GetKSFromRequest();
                
                string language = Utils.Utils.GetLanguageFromRequest();
                
                string udid = ks.ExtractKSData().UDID;
                int domainId = (int)HouseholdUtils.GetHouseholdIDByKS(ks.GroupId);

                response = ClientsManager.CatalogClient().GetAssetCommentsList
                    (ks.GroupId, language, filter.AssetIdEqual, filter.AssetTypeEqual, ks.UserId, domainId, udid, pager.getPageIndex(), pager.PageSize, filter.OrderBy);

                // if no response - return not found status 
                if (response == null || response.Objects == null || response.Objects.Count == 0)
                {
                    throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset-Comment");
                }               
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Add asset comments by asset id
        /// </summary>        
        /// <param name="comment">comment</param>
        /// <remarks></remarks>
        [Action("add")]
        [ApiAuthorize]
        static public KalturaAssetComment Add(KalturaAssetComment comment)
        {
            KalturaAssetComment response = null;

            if (comment.AssetId == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "comment.assetId");
            }

            try
            {
                var ks = KSManager.GetKSFromRequest();
                int groupId = ks.GroupId;
                string userId = ks.UserId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);     
                string udid = ks.ExtractKSData().UDID;
                string language = Utils.Utils.GetLanguageFromRequest();
                
                // call client
                response = ClientsManager.CatalogClient().AddAssetComment(groupId, comment.AssetId, comment.AssetType, userId, (int)domainId, comment.Writer,
                                                                          comment.Header, comment.SubHeader, comment.Text, udid, language);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}