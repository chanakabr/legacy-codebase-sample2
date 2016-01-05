using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebAPI.Catalog;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/personalAsset/action")]
    public class PersonalAssetController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Lists personal enriched data for given assets
        /// </summary>
        /// <param name="assets">Assets and files which we want their data</param>
        /// <param name="with">Which data will be returned</param>
        /// <param name="coupon_code">Discount coupon code</param>
        /// <param name="language">Language code</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaPersonalAssetListResponse List(List<KalturaPersonalAssetRequest> assets, List<KalturaPersonalAssetWithHolder> with,
            string coupon_code = null, string language = null)
        {
            KalturaPersonalAssetListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            if (with == null)
            {
                with = new List<KalturaPersonalAssetWithHolder>();
            }

            try
            {
                // get user id and domain from KS
                string userID = KS.GetFromRequest().UserId;
                int domainId =(int)HouseholdUtils.GetHouseholdIDByKS(groupId);

                Dictionary<string, KalturaPersonalAsset> assetIdToPersonalAsset = new Dictionary<string,KalturaPersonalAsset>();
                Dictionary<long, KalturaPersonalAsset> fileToPersonalAsset = new Dictionary<long, KalturaPersonalAsset>();

                // Create response list to be identical as request
                // +, map ids to objects in response

                response = new KalturaPersonalAssetListResponse();
                response.TotalCount = assets.Count;
                response.Objects = new List<KalturaPersonalAsset>();

                foreach (var asset in assets)
                {
                    var responseAsset = new KalturaPersonalAsset()
                    {
                        Id = asset.Id,
                        Type = asset.Type,
                        Files = new List<KalturaItemPrice>()
                    };

                    // pair example: Key = Media.875638 Value = new and empty personal asset data
                    assetIdToPersonalAsset.Add(string.Format("{0}.{1}", asset.Type.ToString(), asset.Id),
                        responseAsset);

                    // Run on all file IDs and map them to respone asset
                    foreach (var file in asset.FileIds)
                    {
                        fileToPersonalAsset.Add(file, responseAsset);
                    }

                    response.Objects.Add(responseAsset);
                }

                KalturaAssetsBookmarksResponse bookmarksResponse = null;
                List<KalturaItemPrice> pricingsResponse = null;

                // Perform the two calls asynchronously and then merge their two results
                List<Task> taskList = new List<Task>();

                var withTypes = with.Select(x => x.type);

                if (withTypes.Contains(KalturaPersonalAssetWith.bookmark))
                {
                    #region Bookmarks

                    var task = Task.Factory.StartNew(() =>
                    {
                        // Convert request to catalog client's parameter

                        var assetsBookmarksRequest = new List<KalturaSlimAsset>();

                        foreach (var asset in assets)
                        {
                            assetsBookmarksRequest.Add(new KalturaSlimAsset()
                            {
                                Id = asset.Id.ToString(),
                                Type = asset.Type
                            });
                        }

                        // Call catalog
                        bookmarksResponse = ClientsManager.CatalogClient().GetAssetsBookmarks(userID, groupId, domainId, udid, assetsBookmarksRequest);
                    });

                    taskList.Add(task);

                    #endregion
                }

                if (withTypes.Contains(KalturaPersonalAssetWith.pricing))
                {
                    #region Pricing

                    var task = Task.Factory.StartNew(() =>
                    {
                        var fileIds = fileToPersonalAsset.Keys.Select(l => (int)l).ToList();

                        pricingsResponse = ClientsManager.ConditionalAccessClient().GetItemsPrices(groupId, fileIds, userID, coupon_code, udid, language, true);

                    });

                    taskList.Add(task);

                    #endregion
                }

                Task.WaitAll(taskList.ToArray());

                // According to catalog response, update final response's objects
                if (bookmarksResponse != null && bookmarksResponse.AssetsBookmarks != null)
                {
                    foreach (var bookmark in bookmarksResponse.AssetsBookmarks)
                    {
                        string key = string.Format("{0}.{1}", bookmark.Type.ToString(), bookmark.Id);

                        KalturaPersonalAsset personalAsset;

                        if (assetIdToPersonalAsset.TryGetValue(key, out personalAsset))
                        {
                            personalAsset.Bookmarks = bookmark.Bookmarks;
                        }
                    }
                }

                // According to CAS response, update final response's objects
                if (pricingsResponse != null)
                {
                    foreach (var pricing in pricingsResponse)
                    {
                        KalturaPersonalAsset personalAsset;

                        if (fileToPersonalAsset.TryGetValue(pricing.FileId, out personalAsset))
                        {
                            personalAsset.Files.Add(pricing);
                        }
                    }
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