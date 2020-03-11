using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders;
using TVPPro.SiteManager.Helper;
using System.Xml.Serialization;
using System.IO;
using System.Configuration;
using KLogMonitor;
using System.Reflection;
using Core.Catalog.Request;
using Core.Catalog;
using Core.Catalog.Response;
using ConfigurationManager;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class PictureCache : CatalogRequestManager
    {
        //private static Cache m_oCache = new Cache();
        private const string CACHE_KEY_PREFIX = "picture";
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public List<int> PictureIDs { get; set; }

        #region Constructors
        //Construtors for using TVMCatalogProvider:        

        public PictureCache(List<int> pictureIDs, int groupID, string userIP, Filter filter) :
            base(groupID, userIP, 0, 0)
        {
            PictureIDs = pictureIDs;
            m_oFilter = filter;
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new PicRequest()
            {
                m_nPicIds = PictureIDs,
            };
        }

        public object Execute()
        {
            List<BaseObject> retVal = null;
            List<long> picIdsForCatalog = null;

            // Build the List of CacheKeys with DateTime.MinValue
            List<CacheKey> cacheKeys = PictureIDs.Select(picID => new CacheKey() { ID = picID.ToString(), UpdateDate = DateTime.MinValue }).ToList();

            // Get pictures from cache
            Log("Trying to get PictureIDs", PictureIDs);
            List<BaseObject> lPicsFromCache = retVal = CacheManager.Cache.GetObjects(cacheKeys, CACHE_KEY_PREFIX, out picIdsForCatalog);
            Log("Got PictureIDs", lPicsFromCache.Select(pic => pic.AssetId).ToList());

            // Check if pictures are missing in cache 
            if (lPicsFromCache != null && lPicsFromCache.Count > 0)
            {
                if (picIdsForCatalog.Count > 0)
                {
                    // Get missing pictures from Catalog
                    PicRequest thisPicturesRequest = m_oRequest as PicRequest;
                    PicRequest newPicturesRequest = BuildMediasProtocolRequest(picIdsForCatalog.Select(id => (int)id).ToList(), thisPicturesRequest.m_nGroupID, thisPicturesRequest.m_oFilter);
                    retVal = CatalogHelper.MergeObjListsByOrder(PictureIDs, lPicsFromCache, GetPicturesFromCatalog(newPicturesRequest));
                }
                else
                {
                    // Return all the medias from Cache
                    retVal = lPicsFromCache;
                }
            }
            else
            {
                retVal = GetPicturesFromCatalog(m_oRequest as PicRequest);
            }
            return retVal;
        }


        // Get Pictures from Catalog and Store the result Pictures in cache
        private List<BaseObject> GetPicturesFromCatalog(PicRequest request)
        {
            List<BaseObject> retVal = null;
            BaseResponse oPicResponse;
            eProviderResult providerResult = m_oProvider.TryExecuteGetBaseResponse(request, out oPicResponse);
            if (providerResult == eProviderResult.Success && oPicResponse != null && oPicResponse.m_lObj != null && oPicResponse.m_lObj.Count > 0)
            {
                retVal = oPicResponse.m_lObj;
                // Store in Cache the pictures from Catalog
                Log("Got PicResponse from Catalog", oPicResponse);
                Log("Storing Pictures in Cache", oPicResponse.m_lObj);
                int duration = ApplicationConfiguration.TVPApiConfiguration.CacheLiteDurationInMinutes.IntValue;
                CacheManager.Cache.StoreObjects(oPicResponse.m_lObj, CACHE_KEY_PREFIX, duration);
            }
            else if (providerResult != eProviderResult.Success)
            {
                if (!FailOverManager.Instance.SafeMode)
                    FailOverManager.Instance.AddRequest(true);
            }
            return retVal;
        }

        private PicRequest BuildMediasProtocolRequest(List<int> mediaIDs, int groupID, Filter filter)
        {
            PicRequest oRequest = new PicRequest()
            {
                m_nPicIds = PictureIDs,
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
                    case "System.Collections.Generic.List`1[System.Int32]":
                        List<int> picIds = obj as List<int>;
                        sText.AppendLine(CatalogHelper.IDsToString(picIds, "PictureIDs"));
                        break;
                    case "System.Collections.Generic.List`1[Tvinci.Data.Loaders.TvinciPlatform.Catalog.PicObj]":
                        List<PicObj> pics = obj as List<PicObj>;
                        sText.AppendLine(pics.ToStringEx());
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.PicResponse":
                        PicResponse picResponse = obj as PicResponse;
                        sText.AppendLine(picResponse.ToStringEx());
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
            //logger.Info(sText.ToString());
        }
    }
}
