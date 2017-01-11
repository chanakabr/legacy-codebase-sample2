using ApiObjects;
using NPVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Catalog.Response;
using KLogMonitor;
using System.Reflection;

namespace Catalog.Request
{
    [DataContract]
    public class NPVRRetrieveRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public NPVRSearchBy m_eNPVRSearchBy;

        [DataMember]
        public int m_nEPGChannelID;

        [DataMember]
        public List<RecordingStatus> m_lRecordingStatuses;

        [DataMember]
        public List<string> m_lRecordingIDs;

        [DataMember]
        public List<int> m_lProgramIDs;

        [DataMember]
        public DateTime m_dtStartDate;

        [DataMember]
        public RecordedEPGOrderObj m_oOrderObj;

        [DataMember]
        public List<string> m_lSeriesIDs;

        protected override void CheckRequestValidness()
        {
            switch (m_eNPVRSearchBy)
            {
                case NPVRSearchBy.ByRecordingID:
                    if (m_lRecordingIDs == null || m_lRecordingIDs.Count == 0)
                        throw new ArgumentException("No recording IDs provided.");
                    break;
                case NPVRSearchBy.ByRecordingStatus:
                    if (m_lRecordingStatuses == null || m_lRecordingStatuses.Count == 0)
                        throw new ArgumentException("No recording statuses provided.");
                    break;
                default:
                    break;
            }

        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {

            NPVRRetrieveResponse res = new NPVRRetrieveResponse();
            try
            {
                CheckRequestValidness();
                CheckSignature(this);
                if (m_oOrderObj == null)
                {
                    m_oOrderObj = new RecordedEPGOrderObj();
                }

                INPVRProvider npvr;
                if (!NPVRProviderFactory.Instance().IsGroupHaveNPVRImpl(m_nGroupID, out npvr))
                {
                    throw new ArgumentException(String.Concat("GroupID: ", m_nGroupID, " has no NPVR configured."));
                }

                res.recordedProgrammes = Catalog.GetRecordings(m_nGroupID, this, npvr);
                res.m_nTotalItems = res.recordedProgrammes.Count;

            }
            catch (Exception ex)
            {
                log.Error("Exception - " + string.Format("Exception at GetResponse. Req: {0} , Ex Msg: {1} , Ex Type: {2} , ST: {3}", ToString(), ex.Message, ex.GetType().Name, ex.StackTrace), ex);
                throw ex;
            }

            return res;
        }
    }
}
