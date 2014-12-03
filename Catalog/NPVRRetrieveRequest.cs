using ApiObjects;
using NPVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog
{
    [DataContract]
    public class NPVRRetrieveRequest : BaseRequest, IRequestImp
    {
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
        
        protected override void CheckRequestValidness()
        {
            if (!NPVRProviderFactory.Instance().IsGroupHaveNPVRImpl(m_nGroupID))
            {
                throw new ArgumentException(String.Concat("GroupID: ", m_nGroupID, " has no NPVR configured."));
            }
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
                res.recordedProgrammes = Catalog.GetRecordings(m_nGroupID, this);
                res.m_nTotalItems = res.recordedProgrammes.Count;


            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Exception", string.Format("Exception at GetResponse. Req: {0} , Ex Msg: {1} , Ex Type: {2} , ST: {3}", ToString(), ex.Message, ex.GetType().Name, ex.StackTrace), "NPVRRetrieveRequest");
                throw ex;
            }

            return res;
        }
    }
}
