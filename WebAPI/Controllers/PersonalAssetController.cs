using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebAPI.Catalog;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/personalAsset/action")]
    [Obsolete]
    public class PersonalAssetController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Lists personal enriched data for given assets.
        /// <remarks>Possible status codes: FileToMediaMismatch = 3028, InvalidAssetType = 4021, UserNotExistsInDomain = 1020, InvalidUser = 1026</remarks>
        /// </summary>
        /// <param name="assets">Assets and files which we want their data</param>
        /// <param name="with">Which data will be returned</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(WebAPI.Managers.Models.StatusCode.DuplicateAsset)]
        [Throws(WebAPI.Managers.Models.StatusCode.DuplicateFile)]
        public KalturaPersonalAssetListResponse List(List<KalturaPersonalAssetRequest> assets, List<KalturaPersonalAssetWithHolder> with)
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
                int domainId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);
                string language = Utils.Utils.GetLanguageFromRequest();

                Dictionary<string, KalturaPersonalAsset> assetIdToPersonalAsset = new Dictionary<string, KalturaPersonalAsset>();
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

                    string dictionaryKey = string.Format("{0}.{1}", asset.Type.ToString().ToLower(), asset.Id);

                    if (assetIdToPersonalAsset.ContainsKey(dictionaryKey))
                    {
                        throw new BadRequestException(BadRequestException.DUPLICATE_ASSET, asset.Id.ToString(), asset.Type.ToString());
                    }
                    else
                    {
                        // pair example: Key = Media.875638 Value = new and empty personal asset data
                        assetIdToPersonalAsset.Add(dictionaryKey, responseAsset);
                    }

                    if (asset.FileIds != null)
                    {
                        // Run on all file IDs and map them to response asset
                        foreach (long file in asset.FileIds)
                        {
                            if (fileToPersonalAsset.ContainsKey(file))
                            {
                                throw new BadRequestException(BadRequestException.DUPLICATE_FILE, file.ToString());
                            }
                            else
                            {
                                fileToPersonalAsset.Add(file, responseAsset);
                            }
                        }
                    }

                    response.Objects.Add(responseAsset);
                }

                KalturaAssetsBookmarksResponse bookmarksResponse = null;
                List<KalturaAssetPrice> pricingsResponse = null;
                List<KalturaSlimAsset> followingAssets = null;

                // Perform the two calls asynchronously and then merge their two results

                var withTypes = with.Select(x => x.type);

                HttpContext ctx = HttpContext.Current;
                List<Task> taskList = new List<Task>();

                if (withTypes.Contains(KalturaPersonalAssetWith.bookmark))
                {
                    #region Bookmarks

                    var task = Task.Factory.StartNew(() =>
                    {
                        HttpContext.Current = ctx;

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
                        bookmarksResponse = ClientsManager.CatalogClient().GetAssetsBookmarksOldStandard(userID, groupId, domainId, udid, assetsBookmarksRequest);
                    });

                    taskList.Add(task);

                    #endregion
                }

                if (withTypes.Contains(KalturaPersonalAssetWith.pricing))
                {
                    #region Pricing

                    var task = Task.Factory.StartNew(() =>
                    {
                        HttpContext.Current = ctx;

                        var fileIds = fileToPersonalAsset.Keys.Select(l => (int)l).ToList();

                        pricingsResponse = ClientsManager.ConditionalAccessClient().GetAssetPrices(groupId, userID, string.Empty, language, udid, assets);

                    });

                    taskList.Add(task);

                    #endregion
                }

                if (withTypes.Contains(KalturaPersonalAssetWith.following))
                {
                    #region Following

                    var task = Task.Factory.StartNew(() =>
                    {
                        HttpContext.Current = ctx;

                        // get all phrases.
                        //List<string> followingPhrases;
                        var userTvSeries = ClientsManager.NotificationClient().GetUserTvSeriesFollows(groupId, userID, 1000, 0, null);
                        if (userTvSeries != null && userTvSeries.FollowDataList != null && userTvSeries.FollowDataList.Count > 0)
                        {
                            //followingPhrases = userTvSeries.FollowDataList.Select(x => x.FollowPhrase).ToList();

                            // Call catalog
                            //followingAssets = ClientsManager.CatalogClient().GetAssetsFollowing(userID, groupId, assets, followingPhrases);
                            followingAssets = new List<KalturaSlimAsset>();
                            foreach (var item in userTvSeries.FollowDataList)
                            {
                                var asset = assets.Where(x => x.Id == item.AssetId).FirstOrDefault();
                                if (asset != null)
                                    followingAssets.Add(new KalturaSlimAsset() { Type = KalturaAssetType.media, Id = asset.Id.ToString() });
                            }
                        }
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
                        string key = string.Format("{0}.{1}", bookmark.Type.ToString().ToLower(), bookmark.Id);

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
                        string key = string.Format("{0}.{1}", pricing.AssetType.ToString().ToLower(), pricing.AssetId);

                        KalturaPersonalAsset personalAsset;

                        if (assetIdToPersonalAsset.TryGetValue(key, out personalAsset))
                        {
                            personalAsset.Files = pricing.FilePrices;
                        }
                    }
                }

                // According to unified search response, update final response's objects
                if (followingAssets != null && followingAssets.Count > 0)
                {
                    foreach (var asset in followingAssets)
                    {
                        string key = string.Format("{0}.{1}", asset.Type.ToString().ToLower(), asset.Id);

                        KalturaPersonalAsset personalAsset;

                        if (assetIdToPersonalAsset.TryGetValue(key, out personalAsset))
                        {
                            personalAsset.Following = true;
                        }
                    }
                }
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions != null)
                {
                    foreach (var exInner in ex.InnerExceptions)
                    {
                        if (exInner is ClientException)
                        {
                            ErrorUtils.HandleClientException(exInner as ClientException);
                        }
                        else
                        {
                            throw exInner;
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