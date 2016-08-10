using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/assetComment/action")]
    public class AssetCommentController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        /// <summary>
        /// Returns Epg Comments by EpgId
        /// </summary>
        /// <param name="filter">Filtering the assets comments request</param>
        /// <param name="pager">Page size and index</param>
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetCommentListResponse List(KalturaAssetCommentFilter filter, KalturaFilterPager pager = null)
        {
            KalturaAssetCommentListResponse response = null;
           
            if (filter.AssetIdEqual <= 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id cannot have zero or less value");
            }
            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string language = Utils.Utils.GetLanguageFromRequest();
                string userId = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                int domainId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);

                response = ClientsManager.CatalogClient().GetAssetCommentsList(groupId, language, filter.AssetIdEqual, filter.AssetTypeEqual, userId, domainId, udid, pager.getPageIndex(), pager.PageSize,
                    filter.OrderBy);

                // if no response - return not found status 
                if (response == null || response.Objects == null || response.Objects.Count == 0)
                {
                    throw new NotFoundException();
                }               
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}