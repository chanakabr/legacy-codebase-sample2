using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.TvinciPlatform.api;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class EPGLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(EPGSearchLoader));

        

        protected EPGCache m_oEPGCache;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public EpgSearchType SearchType { get; set; }
        public List<int> ChannelIDs { get; set; }
        public int NextTop { get; set; }
        public int PrevTop { get; set; }


        #region Constructors

        public EPGLoader(int groupID, string userIP, int pageSize, int pageIndex, List<int> channelIDs, EpgSearchType searchType, DateTime startTime, DateTime endTime, int nextTop, int prevTop)
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
            List<EPGMultiChannelProgrammeObject> retVal = null;
            if (m_oResponse != null && ((EpgResponse)m_oResponse).programIDsPerChannel != null && ((EpgResponse)m_oResponse).programIDsPerChannel.Count > 0)
            {
                retVal = new List<EPGMultiChannelProgrammeObject>();
                List<BaseObject> lProgramObj = null;
                foreach (var progIDs in ((EpgResponse)m_oResponse).programIDsPerChannel)
                {
                    EPGMultiChannelProgrammeObject epgMultiChannelProgrammeObject = new EPGMultiChannelProgrammeObject();
                    epgMultiChannelProgrammeObject.EPG_CHANNEL_ID = progIDs.m_nCHannelID.ToString();
                    

                    m_oEPGCache = new EPGCache(progIDs.m_resultIDs, GroupID, m_sUserIP, m_oFilter);
                    m_oEPGCache.BuildRequest();
                    lProgramObj = (List<BaseObject>)m_oEPGCache.Execute();

                    epgMultiChannelProgrammeObject.EPGChannelProgrammeObject = (TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject[])ExecuteEPGAdapter(lProgramObj);
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

        protected virtual object ExecuteEPGAdapter(List<BaseObject> programs)
        {
            List<TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject> retVal = null;
            if (programs != null)
            {
                retVal = new List<TvinciPlatform.api.EPGChannelProgrammeObject>();
                foreach (ProgramObj program in programs)
                {
                    var prog = new TvinciPlatform.api.EPGChannelProgrammeObject()
                    {
                        CREATE_DATE = program.m_oProgram.CREATE_DATE,
                        DESCRIPTION = program.m_oProgram.DESCRIPTION,
                        END_DATE = program.m_oProgram.END_DATE,
                        EPG_CHANNEL_ID = program.m_oProgram.EPG_CHANNEL_ID,
                        EPG_ID = program.m_oProgram.EPG_ID,
                        EPG_IDENTIFIER = program.m_oProgram.EPG_IDENTIFIER,
                        GROUP_ID = program.m_oProgram.GROUP_ID,
                        IS_ACTIVE = program.m_oProgram.IS_ACTIVE,
                        LIKE_COUNTER = program.m_oProgram.LIKE_COUNTER,
                        media_id = program.m_oProgram.media_id,
                        NAME = program.m_oProgram.NAME,
                        PIC_URL = program.m_oProgram.PIC_URL,
                        PUBLISH_DATE = program.m_oProgram.PUBLISH_DATE,
                        START_DATE = program.m_oProgram.START_DATE,
                        STATUS = program.m_oProgram.STATUS,
                        UPDATE_DATE = program.m_oProgram.UPDATE_DATE,
                        UPDATER_ID = program.m_oProgram.UPDATER_ID
                    };

                    if (program.m_oProgram.EPG_Meta != null)
                    {
                        var metas = new List<TvinciPlatform.api.EPGDictionary>();
                        foreach (var meta in program.m_oProgram.EPG_Meta)
                        {
                            metas.Add(new TVPPro.SiteManager.TvinciPlatform.api.EPGDictionary { Key = meta.Key, Value = meta.Value});
                        }
                        prog.EPG_Meta = metas.ToArray();
                    }

                    if (program.m_oProgram.EPG_TAGS != null)
                    {
                        var tags = new List<TvinciPlatform.api.EPGDictionary>();
                        foreach (var tag in program.m_oProgram.EPG_TAGS)
                        {
                            tags.Add(new TVPPro.SiteManager.TvinciPlatform.api.EPGDictionary { Key = tag.Key, Value = tag.Value });
                        }
                        prog.EPG_Meta = tags.ToArray();
                    }
                    retVal.Add(prog);
                }

            }
            return retVal.ToArray();
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
