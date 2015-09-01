using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Objects.Responses;

namespace TVPApiModule.CatalogLoaders
{
    public class APIAutocompleteLoader : APIUnifiedSearchLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Ctor

        public APIAutocompleteLoader(int groupID, PlatformType platform, int domainId, string userIP, int pageSize, int pageIndex,
            List<int> assetTypes, string filter, List<string> with, List<ePersonalFilter> personalFilters)
            : base(groupID, platform, domainId, userIP, pageSize, pageIndex, assetTypes, filter, with, personalFilters)
        {
        }

        #endregion

        #region Override Methods

        protected override object Process()
        {
            TVPApiModule.Objects.Responses.AutocompleteResponse result = null;

            string cacheKey = GetLoaderCachekey();

            // No response from Catalog, gets medias from cache
            if (m_oResponse == null)
            {
                m_oResponse = CacheManager.Cache.GetFailOverResponse(cacheKey);

                // No response from Catalog and no response from cache
                if (m_oResponse == null)
                {
                    result = new Objects.Responses.AutocompleteResponse();
                    result.Status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
                    return result;
                }
            }

            Tvinci.Data.Loaders.TvinciPlatform.Catalog.UnifiedSearchResponse response = (Tvinci.Data.Loaders.TvinciPlatform.Catalog.UnifiedSearchResponse)m_oResponse;

            // Bad response from Catalog - return the status
            if (response.status.Code != (int)eStatus.OK)
            {
                result = new Objects.Responses.AutocompleteResponse();
                result.Status = new Objects.Responses.Status((int)response.status.Code, response.status.Message);
                return result;
            }

            // Add the status and the number of total items to the response
            result = new Objects.Responses.AutocompleteResponse();
            result.Status = new Objects.Responses.Status((int)response.status.Code, response.status.Message);
            result.TotalItems = response.m_nTotalItems;

            if (response.searchResults != null && response.searchResults.Count > 0)
            {
                List<MediaObj> medias;
                List<ProgramObj> epgs;

                GetAssets(cacheKey, response, out medias, out epgs);

                // Gets one list including both medias and EPGs, ordered by Catalog order
                result.Assets = OrderAndCompleteSlimResults(response.searchResults, medias, epgs);
            }
            else
            {
                result.Assets = new List<SlimAssetInfo>();
            }

            return result;
        }

        /// <summary>
        /// Create a list of slim asset info objects in their original order
        /// </summary>
        /// <param name="assetsList"></param>
        /// <param name="medias"></param>
        /// <param name="epgs"></param>
        /// <returns></returns>
        protected List<SlimAssetInfo> OrderAndCompleteSlimResults(List<UnifiedSearchResult> assetsList, List<MediaObj> medias, List<ProgramObj> epgs)
        {
            if (assetsList == null || ((medias == null || medias.Count == 0) && (epgs == null || epgs.Count == 0)))
            {
                return null;
            }

            // Convert lists to dictionaries for easier access by ID later on
            Dictionary<string, MediaObj> idToMedia = new Dictionary<string, MediaObj>();
            Dictionary<string, ProgramObj> idToEpg = new Dictionary<string,ProgramObj>();

            if (medias != null)
            {
                medias.ForEach(media =>
                {
                    idToMedia[media.AssetId] = media;
                });
            }

            if (epgs != null)
            {
                epgs.ForEach(epg =>
                {
                    idToEpg[epg.AssetId] = epg;
                });
            }

            List<SlimAssetInfo> result = new List<SlimAssetInfo>();

            bool shouldAddImages = false;

            if (this.With != null)
            {
                // If images are required - get them
                if (this.With.Contains("images"))
                {
                    shouldAddImages = true;
                }
            }

            // Build the AssetInfo objects in the specific order
            foreach (UnifiedSearchResult item in assetsList)
            {
                SlimAssetInfo asset = null;
                MediaObj media = null;
                ProgramObj epg = null;

                if (item.AssetType == Tvinci.Data.Loaders.TvinciPlatform.Catalog.eAssetTypes.MEDIA)
                {
                    if (idToMedia.ContainsKey(item.AssetId))
                    {
                        media = idToMedia[item.AssetId];

                        asset = new SlimAssetInfo(media, shouldAddImages);
                        result.Add(asset);

                        media = null;
                    }
                }
                else if (item.AssetType == Tvinci.Data.Loaders.TvinciPlatform.Catalog.eAssetTypes.EPG)
                {
                    if (idToEpg.ContainsKey(item.AssetId))
                    {
                        epg = idToEpg[item.AssetId];

                        asset = new SlimAssetInfo(epg.m_oProgram, shouldAddImages);

                        result.Add(asset);
                        epg = null;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
