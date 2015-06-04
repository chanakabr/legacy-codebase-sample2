using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using Tvinci.Data.Loaders;
using TVPPro.SiteManager.Helper;
using System.Configuration;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class MediaCache : CatalogRequestManager
    {
        private const string CACHE_KEY_PREFIX = "media";
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        public List<SearchResult> MediaIDs { get; set; }

        #region Constructors
        //Constructors for using TVMCatalogProvider:        

        public MediaCache(List<SearchResult> mediaIDs, int groupID, string userIP, Filter filter) :
            base(groupID, userIP, 0, 0)
        {
            MediaIDs = mediaIDs;
            m_oFilter = filter;

        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new MediasProtocolRequest()
            {
                m_lMediasIds = MediaIDs.Select(media => media.assetID).ToList(),
            };
        }

        public object Execute()
        {
            List<BaseObject> retVal = null;
            List<long> mediaIdsForCatalog = null;

            // Build the List of CacheKeys from the MediaRes List
            List<CacheKey> cacheKeys = MediaIDs.Select(mediaRes => new CacheKey() { ID = mediaRes.assetID.ToString(), UpdateDate = mediaRes.UpdateDate }).ToList();

            // Get medias from cache
            Log("Trying to get mediaIDs", MediaIDs);
            List<BaseObject> lMediasFromCache = retVal = CacheManager.Cache.GetObjects(cacheKeys, string.Format("{0}_lng{1}", CACHE_KEY_PREFIX, Language), out mediaIdsForCatalog);
            Log("Got mediaIDs", lMediasFromCache.Select(media => media.AssetId).ToList());

            // Check if medias are missing in cache 
            if (lMediasFromCache != null && lMediasFromCache.Count > 0)
            {
                if (mediaIdsForCatalog.Count > 0 && !FailOverManager.Instance.SafeMode)
                {
                    // Get missing medias from Catalog
                    MediasProtocolRequest thisMediasRequest = m_oRequest as MediasProtocolRequest;
                    MediasProtocolRequest newMediasRequest = BuildMediasProtocolRequest(mediaIdsForCatalog.Select(id => (int)id).ToList(), thisMediasRequest.m_nGroupID, thisMediasRequest.m_oFilter);
                    // marge the lists
                    retVal = CatalogHelper.MergeObjListsByOrder(MediaIDs.Select(media => media.assetID).ToList(), lMediasFromCache, GetMediasFromCatalog(newMediasRequest));
                }
                else
                {
                    // Return all the medias from Cache
                    retVal = lMediasFromCache;
                }
            }
            else
            {
                retVal = GetMediasFromCatalog(m_oRequest as MediasProtocolRequest);
            }
            return retVal;
        }

        // Get Medias from Catalog and Store the result Medias in cache
        private List<BaseObject> GetMediasFromCatalog(MediasProtocolRequest request)
        {
            List<BaseObject> retVal = null;

            MediaResponse oMediaResponse;
            eProviderResult providerResult = m_oProvider.TryExecuteGetMediasByIDs(request, out oMediaResponse);
            if (providerResult == eProviderResult.Success && oMediaResponse != null && oMediaResponse.m_lObj != null && oMediaResponse.m_lObj.Count > 0)
            {
                retVal = oMediaResponse.m_lObj;
                // Store in Cache the medias from Catalog
                Log("Got MediaResponse from Catalog", oMediaResponse);
                Log("Storing Medias in Cache", oMediaResponse.m_lObj);
                int duration;
                int.TryParse(ConfigurationManager.AppSettings["Tvinci.DataLoader.CacheLite.DurationInMinutes"], out duration);
                CacheManager.Cache.StoreObjects(oMediaResponse.m_lObj, string.Format("{0}_lng{1}", CACHE_KEY_PREFIX, Language), duration);
            }

            return retVal;
        }

        private MediasProtocolRequest BuildMediasProtocolRequest(List<int> mediaIDs, int groupID, Filter filter)
        {
            MediasProtocolRequest oRequest = new MediasProtocolRequest()
            {
                m_lMediasIds = mediaIDs,
                m_nGroupID = groupID,
                m_sSignature = m_sSignature,
                m_sSignString = m_sSignString,
                m_sUserIP = string.Empty,
                m_oFilter = filter
            };
            return oRequest;
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "System.Collections.Generic.List`1[Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaRes]":
                        List<SearchResult> lMediaRes = obj as List<SearchResult>;
                        sText.AppendLine(lMediaRes.ToStringEx());
                        break;
                    case "System.Collections.Generic.List`1[System.Int32]":
                        List<int> mediaIds = obj as List<int>;
                        sText.AppendLine(CatalogHelper.IDsToString(mediaIds, "MediaIDs"));
                        break;
                    case "System.Collections.Generic.List`1[Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaObj]":
                        List<MediaObj> medias = obj as List<MediaObj>;
                        sText.AppendLine(medias.ToStringEx());
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaResponse":
                        MediaResponse mediaResponse = obj as MediaResponse;
                        sText.AppendLine(mediaResponse.ToStringEx());
                        //XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
                        //StringWriter textWriter = new StringWriter();
                        //xmlSerializer.Serialize(textWriter, obj);
                        //sText.AppendLine(textWriter.ToString());
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
        }
    }
}
