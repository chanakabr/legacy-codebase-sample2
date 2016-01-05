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
                        fileToPersonalAsset.Add(file.value, responseAsset);
                    }

                    response.Objects.Add(responseAsset);
                }

                var withTypes = with.Select(x => x.type);

                if (withTypes.Contains(KalturaPersonalAssetWith.bookmark))
                {
                    #region Bookmarks

                    // Convert request to catalog client's parameter

                    var assetsBookmarksRequest = new List<AssetBookmarkRequest>();

                    foreach (var asset in assets)
                    {
                        eAssetTypes type = eAssetTypes.UNKNOWN;

                        switch (asset.Type)
                        {
                            case KalturaAssetType.media:
                            {
                                type = eAssetTypes.MEDIA;
                                break;
                            }
                            case KalturaAssetType.recording:
                            {
                                type = eAssetTypes.NPVR;
                                break;
                            }
                            case KalturaAssetType.epg:
                            {
                                type = eAssetTypes.EPG;
                                break;
                            }
                            default:
                            break;
                        }

                        assetsBookmarksRequest.Add(new AssetBookmarkRequest()
                        {
                            AssetID = asset.Id.ToString(),
                            AssetType = type
                        });
                    }

                    // Call catalog
                    var bookmarksResponse =
                        ClientsManager.CatalogClient().GetAssetsBookmarks(userID, groupId, domainId, udid, assetsBookmarksRequest);

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

                    #endregion
                }

                if (withTypes.Contains(KalturaPersonalAssetWith.pricing))
                {
                    #region Pricing

                    var pricingsResponse = ClientsManager.ConditionalAccessClient().GetItemsPrices(groupId, null, userID, coupon_code, udid, language, true);

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

                    #endregion
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