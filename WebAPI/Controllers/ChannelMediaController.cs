using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/channelMedia/action")]
    public class ChannelMediaController : ApiController
    {
        /// <summary>
        /// Returns all channel media        
        /// </summary>
        /// <param name="channel_id">Channel identifier</param>        
        /// <param name="order_by">Required sort option to apply for the identified assets. If omitted – will use channel default ordering.</param>        
        /// <param name="pager"><![CDATA[Page size and index. Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted - will be set to 25. If a value > 50 provided – will set to 50]]></param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="language">Language code</param>                
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize(true)]
        public KalturaAssetInfoListResponse List(int channel_id, KalturaOrder? order_by = null, KalturaFilterPager pager = null,
            List<KalturaCatalogWithHolder> with = null, string language = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (channel_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "channel_id cannot be 0");
            }

            if (pager == null)
                pager = new KalturaFilterPager();

            // Size rules - according to spec.  10>=size>=1 is valid. default is 5.
            if (pager.PageSize > 10 || pager.PageSize < 1)
            {
                pager.PageSize = 5;
            }

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            try
            {
                string userID = KS.GetFromRequest().UserId;

                response = ClientsManager.CatalogClient().GetChannelMedia(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), string.Empty, language,
                    pager.PageIndex, pager.PageSize, channel_id, order_by, with.Select(x => x.type).ToList());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}