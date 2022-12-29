using Phx.Lib.Log;
using System;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Social;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.Utils;
using WebAPI.ModelsValidators;

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
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.SocialClient().GetSocialFeed(groupId, KS.GetFromRequest().UserId, filter.AssetIdEqual, filter.AssetTypeEqual, filter.SocialPlatformEqual,
                    pager.PageSize.Value, pager.GetRealPageIndex(), filter.CreateDateGreaterThan, filter.OrderBy);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}