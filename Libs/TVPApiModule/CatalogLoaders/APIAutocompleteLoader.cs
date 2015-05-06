using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Objects.Responses;

namespace TVPApiModule.CatalogLoaders
{
    public class APIAutocompleteLoader : APIUnifiedSearchLoader
    {
        #region Data Members

        private static ILog logger = log4net.LogManager.GetLogger(typeof(APIAutocompleteLoader));

        #endregion

        #region Ctor

        public APIAutocompleteLoader(int groupID, PlatformType platform, int domainId, string userIP, int pageSize, int pageIndex,
            List<int> assetTypes, string filter, List<string> with)
            : base(groupID, platform, domainId, userIP, pageSize, pageIndex, assetTypes, filter, with)
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
            Dictionary<int, MediaObj> idToMedia = null;
            Dictionary<int, ProgramObj> idToEpg = null;

            if (medias != null)
            {
                idToMedia = medias.ToDictionary<MediaObj, int>(media => media.m_nID);
            }
            else
            {
                idToMedia = new Dictionary<int, MediaObj>();
            }

            if (epgs != null)
            {
                idToEpg = epgs.ToDictionary<ProgramObj, int>(epg => epg.m_nID);
            }
            else
            {
                idToEpg = new Dictionary<int, ProgramObj>();
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

                if (item.type == Tvinci.Data.Loaders.TvinciPlatform.Catalog.AssetType.Media)
                {
                    if (idToMedia.ContainsKey(item.assetID))
                    {
                        media = idToMedia[item.assetID];

                        asset = new SlimAssetInfo(media, shouldAddImages);
                        result.Add(asset);

                        media = null;
                    }
                }
                else if (item.type == Tvinci.Data.Loaders.TvinciPlatform.Catalog.AssetType.Epg)
                {
                    if (idToEpg.ContainsKey(item.assetID))
                    {
                        epg = idToEpg[item.assetID];

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
