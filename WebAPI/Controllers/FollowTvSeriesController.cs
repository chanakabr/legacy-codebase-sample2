using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/homeNetwork/action")]
    public class FollowTvSeriesController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="order_by"></param>
        /// <param name="pager"></param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaListFollowDataResponse List(KalturaOrder? order_by = null, KalturaFilterPager pager = null)
        {
            KalturaListFollowDataResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (pager == null)
                pager = new KalturaFilterPager();

            string userID = KS.GetFromRequest().UserId;

            try
            {
                response = ClientsManager.NotificationClient().GetUserTvSeriesFollows(groupId, userID, pager.PageSize, pager.PageIndex, order_by);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset_id"></param>
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(int asset_id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            string userID = KS.GetFromRequest().UserId;

            try
            {
                response = ClientsManager.NotificationClient().DeleteUserTvSeriesFollow(groupId, userID, asset_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset_id"></param>
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public bool Add(int asset_id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            string userID = KS.GetFromRequest().UserId;

            try
            {
                response = ClientsManager.NotificationClient().AddUserTvSeriesFollow(groupId, userID, asset_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}