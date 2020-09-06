using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiObjects.SearchObjects;
using ConfigurationManager;
using Core.Catalog;
using Core.Catalog.Request;
using Core.Catalog.Response;
using KLogMonitor;
using Tvinci.Data.Loaders;
using TVPPro.SiteManager.Helper;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class EPGCache : CatalogRequestManager
    {
        //private static Cache m_oCache = new Cache();
        private const string CACHE_KEY_PREFIX = "epg";

        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public List<SearchResult> ProgramIDs { get; set; }

        #region Constructors
        //Constructors for using TVMCatalogProvider:        

        public EPGCache(List<SearchResult> programIDs, int groupID, string userIP, Filter filter) :
            base(groupID, userIP, 0, 0)
        {
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
            return GetProgramsFromCatalog(m_oRequest as EpgProgramDetailsRequest);
        }

        // Get Programs from Catalog and Store the result Programs in cache
        private List<BaseObject> GetProgramsFromCatalog(EpgProgramDetailsRequest request)
        {
            List<BaseObject> retVal = null;
            EpgProgramResponse oProgramResponse;
            if (m_oProvider.TryExecuteGetProgramsByIDs(request, out oProgramResponse) == eProviderResult.Success && oProgramResponse != null && oProgramResponse.m_lObj != null && oProgramResponse.m_lObj.Count > 0)
            {
                retVal = oProgramResponse.m_lObj;
            }
            return retVal;
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
