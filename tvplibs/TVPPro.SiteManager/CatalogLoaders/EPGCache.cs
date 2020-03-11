using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Helper;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class EPGCache : CatalogRequestManager
    {
        //private static Cache m_oCache = new Cache();
        private const string CACHE_KEY_PREFIX = "epg";
        private static int Duration;

        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        public List<SearchResult> ProgramIDs { get; set; }

        #region Constructors
        //Constructors for using TVMCatalogProvider:        

        public EPGCache(List<SearchResult> programIDs, int groupID, string userIP, Filter filter) :
            base(groupID, userIP, 0, 0)
        {
            int.TryParse(ConfigurationManager.AppSettings["Tvinci.DataLoader.CacheLite.DurationInMinutes"], out Duration);

            ProgramIDs = programIDs;
            m_oFilter = filter;
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new EpgProgramDetailsRequest()
            {
                m_lProgramsIds = ProgramIDs.Select(program => program.assetID).ToList()
            };

        }

        public object Execute()
        {
            List<BaseObject> retVal = null;
            List<long> programIdsForCatalog = null;

            // Build the List of CacheKeys from the ProgramRes List
            List<CacheKey> cacheKeys = ProgramIDs.Select(programRes => new CacheKey() { ID = programRes.assetID.ToString(), UpdateDate = programRes.UpdateDate }).ToList();

            // Get programs from cache
            Log("Trying to get programIDs", ProgramIDs);
            List<BaseObject> lProgramsFromCache = retVal = CacheManager.Cache.GetObjects(cacheKeys, string.Format("{0}_lng{1}", CACHE_KEY_PREFIX, Language), out programIdsForCatalog);
            Log("Got programIDs", lProgramsFromCache.Select(program => program.AssetId).ToList());

            // Check if programs are missing in cache 
            if (lProgramsFromCache != null && lProgramsFromCache.Count > 0)
            {
                if (programIdsForCatalog.Count > 0)
                {
                    // Get missing programs from Catalog
                    EpgProgramDetailsRequest thisProgramsRequest = m_oRequest as EpgProgramDetailsRequest;
                    EpgProgramDetailsRequest newProgramsRequest = BuildProgramsProtocolRequest(programIdsForCatalog.Select(id => (int)id).ToList(), thisProgramsRequest.m_nGroupID, thisProgramsRequest.m_oFilter);
                    retVal = CatalogHelper.MergeObjListsByOrder(ProgramIDs.Select(program => program.assetID).ToList(), lProgramsFromCache, GetProgramsFromCatalog(newProgramsRequest));
                }
                else
                {
                    // Return all the programs from Cache
                    retVal = lProgramsFromCache;
                }
            }
            else
            {
                retVal = GetProgramsFromCatalog(m_oRequest as EpgProgramDetailsRequest);
            }
            return retVal;
        }

        // Get Programs from Catalog and Store the result Programs in cache
        private List<BaseObject> GetProgramsFromCatalog(EpgProgramDetailsRequest request)
        {
            List<BaseObject> retVal = null;
            EpgProgramResponse oProgramResponse;
            if (m_oProvider.TryExecuteGetProgramsByIDs(request, out oProgramResponse) == eProviderResult.Success && oProgramResponse != null && oProgramResponse.m_lObj != null && oProgramResponse.m_lObj.Count > 0)
            {
                retVal = oProgramResponse.m_lObj;
                // Store in Cache the programs from Catalog
                Log("Got ProgramResponse from Catalog", oProgramResponse);
                Log("Storing Programs in Cache", oProgramResponse.m_lObj);
                CacheManager.Cache.StoreObjects(oProgramResponse.m_lObj, string.Format("{0}_lng{1}", CACHE_KEY_PREFIX, Language), Duration);
            }
            return retVal;
        }

        private EpgProgramDetailsRequest BuildProgramsProtocolRequest(List<int> programIDs, int groupID, Filter filter)
        {
            EpgProgramDetailsRequest oRequest = new EpgProgramDetailsRequest()
            {
                m_lProgramsIds = programIDs,
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
                    case "System.Collections.Generic.List`1[Tvinci.Data.Loaders.TvinciPlatform.Catalog.EpgRes]":
                        List<SearchResult> lEpgRes = obj as List<SearchResult>;
                        sText.AppendLine(lEpgRes.ToStringEx());
                        break;
                    case "System.Collections.Generic.List`1[System.Int32]":
                        List<int> epgIds = obj as List<int>;
                        sText.AppendLine(CatalogHelper.IDsToString(epgIds, "EpgIDs"));
                        break;
                    case "System.Collections.Generic.List`1[Tvinci.Data.Loaders.TvinciPlatform.Catalog.ProgramObj]":
                        List<ProgramObj> programs = obj as List<ProgramObj>;
                        sText.AppendLine(programs.ToStringEx());
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.EpgProgramResponse":
                        EpgProgramResponse programResponse = obj as EpgProgramResponse;
                        sText.AppendLine(programResponse.ToStringEx());
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
        }
    }
}
