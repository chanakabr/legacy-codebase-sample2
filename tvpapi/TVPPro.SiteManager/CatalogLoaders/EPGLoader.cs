using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiObjects;
using KLogMonitor;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using Core.Users;
using Core.ConditionalAccess;
using ApiObjects.SearchObjects;
using Core.Catalog;
using Core.Catalog.Response;
using Core.Catalog.Request;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.Objects;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class EPGLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected EPGCache m_oEPGCache;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public EpgSearchType SearchType { get; set; }
        public List<int> ChannelIDs { get; set; }
        public int NextTop { get; set; }
        public int PrevTop { get; set; }


        #region Constructors

        public EPGLoader(int groupID, string userIP, int pageSize, int pageIndex, List<int> channelIDs, 
            EpgSearchType searchType, DateTime startTime, DateTime endTime, int nextTop, int prevTop)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            SearchType = searchType;
            StartTime = startTime;
            EndTime = endTime;
            ChannelIDs = channelIDs;
            NextTop = nextTop;
            PrevTop = prevTop;
        }

        public EPGLoader(string userName, string userIP, int pageSize, int pageIndex, List<int> channelIDs, EpgSearchType searchType, DateTime startTime, DateTime endTime, int nextTop, int prevTop)
            : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, channelIDs, searchType, startTime, endTime, nextTop, prevTop)
        {
        }

        #endregion


        protected override void BuildSpecificRequest()
        {
            m_oRequest = new EpgRequest()
            {
                m_dEndDate = EndTime,
                m_dStartDate = StartTime,
                m_eSearchType = SearchType,
                m_nChannelIDs = ChannelIDs,
                m_nNextTop = NextTop,
                m_nPrevTop = PrevTop,
            };
        }

        protected virtual object Process()
        {
            List<BaseObject> retVal = null;
            if (m_oResponse != null && ((EpgResponse)m_oResponse).programsPerChannel != null)
            {
                retVal = new List<BaseObject>();
                foreach (var progIDs in ((EpgResponse)m_oResponse).programsPerChannel)
                {
                    EPGMultiChannelProgrammeObject epgMultiChannelProgrammeObject = new EPGMultiChannelProgrammeObject();
                    epgMultiChannelProgrammeObject.EPG_CHANNEL_ID = progIDs.m_nChannelID.ToString();

                    if (progIDs.m_lEpgProgram != null)
                        epgMultiChannelProgrammeObject.EPGChannelProgrammeObject = progIDs.m_lEpgProgram;

                    retVal.Add(epgMultiChannelProgrammeObject);
                }
            }
            return retVal;
        }

        public virtual object Execute()
        {
            object retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                retVal = (List<BaseObject>)Process();
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
