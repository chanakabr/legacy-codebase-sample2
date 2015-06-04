using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class EPGSearchLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected EPGCache m_oEPGCache;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string SearchText { get; set; }
        public List<long> EPGChannelIDs { get; set; }
        public List<KeyValue> AndList { get; set; }
        public List<KeyValue> OrList { get; set; }
        public bool Exact { get; set; }


        #region Constructors

        public EPGSearchLoader(int groupID, string userIP, int pageSize, int pageIndex, DateTime startTime, DateTime endTime)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            StartTime = startTime;
            EndTime = endTime;
        }

        public EPGSearchLoader(int groupID, string userIP, int pageSize, int pageIndex, string searchText, DateTime startTime, DateTime endTime)
            : this(groupID, userIP, pageSize, pageIndex, startTime, endTime)
        {
            SearchText = searchText;
        }

        public EPGSearchLoader(int groupID, string userIP, int pageSize, int pageIndex, List<KeyValue> andList, List<KeyValue> orList, bool exact, DateTime startTime, DateTime endTime)
            : this(groupID, userIP, pageSize, pageIndex, startTime, endTime)
        {
            AndList = andList;
            OrList = orList;
            Exact = exact;
        }

        public EPGSearchLoader(string userName, string userIP, int pageSize, int pageIndex, string searchText, DateTime startTime, DateTime endTime)
            : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, searchText, startTime, endTime)
        {
        }

        #endregion


        protected override void BuildSpecificRequest()
        {

            m_oRequest = new EpgSearchRequest()
            {
                m_sSearch = SearchText,
                m_dEndDate = EndTime,
                m_dStartDate = StartTime,
                m_oEPGChannelIDs = EPGChannelIDs,
                m_AndList = AndList,
                m_OrList = OrList,
                m_bExact = Exact,
            };
        }


        protected virtual object Process()
        {
            List<BaseObject> lProgramObj = null;
            if (m_oResponse != null && ((EpgSearchResponse)m_oResponse).m_nEpgIds != null && ((EpgSearchResponse)m_oResponse).m_nEpgIds.Count > 0)
            {
                m_oEPGCache = new EPGCache(((EpgSearchResponse)m_oResponse).m_nEpgIds, GroupID, m_sUserIP, m_oFilter);
                m_oEPGCache.BuildRequest();
                lProgramObj = (List<BaseObject>)m_oEPGCache.Execute();
            }
            return lProgramObj;
        }

        public virtual object Execute()
        {
            object retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                m_oResponse.m_lObj = (List<BaseObject>)Process();
            }
            if (m_oResponse != null && m_oResponse.m_lObj != null)
            {
                retVal = m_oResponse.m_lObj;
            }
            else
            {
                retVal = new List<BaseObject>();
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
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.EpgSearchRequest":
                        EpgSearchRequest searchRequest = obj as EpgSearchRequest;
                        sText.AppendFormat("EpgSearchRequest: GroupID = {0}, PageIndex = {1}, PageSize = {2}, searchText = {3} ", searchRequest.m_nGroupID, searchRequest.m_nPageIndex, searchRequest.m_nPageSize, searchRequest.m_sSearch);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.EpgSearchResponse":
                        EpgSearchResponse searchResponse = obj as EpgSearchResponse;
                        sText.AppendFormat("EpgSearchResponse: TotalItems = {0}, ", searchResponse.m_nTotalItems);
                        sText.AppendLine(searchResponse.m_nEpgIds.ToStringEx());
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
        }

        #region ISupportPaging method
        public bool TryGetItemsCount(out long count)
        {
            count = 0;

            if (m_oResponse == null)
                return false;

            count = m_oResponse.m_nTotalItems;

            return true;
        }
        #endregion

        #region ILoaderAdapter not implemented methods
        public bool IsPersist()
        {
            throw new NotImplementedException();
        }

        public object Execute(eExecuteBehaivor behaivor)
        {
            throw new NotImplementedException();
        }

        public object LastExecuteResult
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}
