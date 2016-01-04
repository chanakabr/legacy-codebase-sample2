using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.Catalog;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/asset/action")]
    public class BookmarkController : ApiController
    {
    //    private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    //    [Route("get"), HttpPost]
    //    [ApiAuthorize(true)]
    //    public KalturaAssetInfo Get(string user, int assetId, string assetType)
    //    {
    //        KalturaAssetInfo response = null;

    //        int groupId = KS.GetFromRequest().GroupId;
    //        string udid = KSUtils.ExtractKSPayload().UDID;
    //        int householdId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);
    //        string siteGuid = KS.GetFromRequest().UserId;

    //        if (string.IsNullOrEmpty(user))
    //        {
    //            throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "user cannot be empty");
    //        }

    //        if (string.IsNullOrEmpty(assetType))
    //        {
    //            throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "assetType cannot be empty");
    //        }

    //        if (assetId == 0)
    //        {
    //            throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "assetId cannot be 0");
    //        }

    //        var res = ClientsManager.CatalogClient().GetAssetBookmark(groupId, user, siteGuid, householdId, udid, assetId, assetType);

    //    }

        [Route("set"), HttpPost]
        [ApiAuthorize(true)]
        public bool Set(string assetId, eAssetTypes assetType, long fileId, KalturaPlayerAssetData PlayerAssetData)
        {            
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                int householdId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);
                string siteGuid = KS.GetFromRequest().UserId;
                response = ClientsManager.CatalogClient().SetBookmark(groupId, siteGuid, householdId, udid, assetId, assetType, fileId, PlayerAssetData);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
};