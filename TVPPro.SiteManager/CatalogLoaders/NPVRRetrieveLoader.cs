using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.CatalogLoaders
{
    public class NPVRRetrieveLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public NPVRSearchBy NPVRSearchBy { get; set; }
        public int EPGChannelID { get; set; }
        public RecordingStatus RecordingStatus { get; set; }
        public List<string> RecordingIDs { get; set; }
        public List<int> ProgramIDs { get; set; }
        public DateTime StartDate { get; set; }
        public RecordedEPGOrderObj RecordedEPGOrderObj { get; set; }
        public List<string> SeriesIDs { get; set; }

       
        #region ctr

        public NPVRRetrieveLoader(int groupID, string userIP, string siteGuid, int pageSize, int pageIndex,
            NPVRSearchBy searchBy, int epgChannelID, RecordingStatus recordingStatus, List<string> recordingIDs, List<int> programIDs, List<string> seriesIDs, DateTime startDate, 
            RecordedEPGOrderObj recordedEPGOrderObj)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            NPVRSearchBy = searchBy;
            EPGChannelID = epgChannelID;
            RecordingStatus = recordingStatus;
            RecordingIDs = recordingIDs;
            ProgramIDs = programIDs;
            StartDate = startDate;
            RecordedEPGOrderObj = recordedEPGOrderObj;
            SiteGuid = siteGuid;
            SeriesIDs = seriesIDs;           
        }


        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new NPVRRetrieveRequest()
            {
                m_eNPVRSearchBy = NPVRSearchBy,
                m_nEPGChannelID = EPGChannelID,
                m_lRecordingStatuses = new List<RecordingStatus>() { RecordingStatus },
                m_lRecordingIDs = RecordingIDs,
                m_lProgramIDs = ProgramIDs,
                m_dtStartDate = StartDate,
                m_oOrderObj = RecordedEPGOrderObj,
                m_sSiteGuid = SiteGuid,
                m_lSeriesIDs = SeriesIDs,
               
            };
        }

        public object Execute()
        {
            List<RecordedEPGChannelProgrammeObject> retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                retVal = (m_oResponse as NPVRRetrieveResponse).recordedProgrammes;
            }
            return retVal;
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                if (obj is NPVRRetrieveRequest)
                {
                    sText.AppendFormat("NPVRRetrieveRequest: groupID = {0}, userIP = {1}, NPVRSearchBy = {2}, EPGChannelID = {3}", GroupID, m_sUserIP, NPVRSearchBy, EPGChannelID);
                }
                else if (obj is NPVRRetrieveResponse)
                {
                    NPVRRetrieveResponse assetStatsResponse = obj as NPVRRetrieveResponse;
                    sText.AppendFormat("NPVRRetrieveResponse");
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
