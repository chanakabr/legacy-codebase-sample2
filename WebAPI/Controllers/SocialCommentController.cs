using KLogMonitor;
using System;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Social;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("socialComment")]
    public class SocialCommentController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Get a list of all social comments filtered by asset ID and social platform
        /// </summary>
        /// <param name="filter">Country filter</param>
        /// <param name="pager">Pager</param>
        /// <remarks></remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaSocialCommentListResponse List(KalturaSocialCommentFilter filter, KalturaFilterPager pager = null)
        {
            KalturaSocialCommentListResponse response = null;

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            filter.validate();
            int groupId = KSManager.GetKSFromRequest().GroupId;

            try
            {
                response = ClientsManager.SocialClient().GetSocialFeed(groupId, KSManager.GetKSFromRequest().UserId, filter.AssetIdEqual, filter.AssetTypeEqual, filter.SocialPlatformEqual,
                    pager.getPageSize(), pager.getPageIndex(), filter.CreateDateGreaterThan, filter.OrderBy);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}