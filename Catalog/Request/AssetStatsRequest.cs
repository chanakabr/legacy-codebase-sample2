using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using Logger;
using ApiObjects;
using Catalog.Response;

namespace Catalog.Request
{
    [DataContract]
    public class AssetStatsRequest : BaseRequest, IRequestImp
    {
        
        [DataMember]
        public StatsType m_type;
        
        [DataMember]
        public DateTime m_dStartDate;

        [DataMember]
        public DateTime m_dEndDate;

        [DataMember]
        public List<int> m_nAssetIDs;
              

        public AssetStatsRequest()
            : base()
        {
        }

        public AssetStatsRequest(StatsType type, List<int> nMediaIDs, DateTime dStartDate, DateTime dEndDate, int nGroupID, int nPageSize, int nPageIndex, string sUserIP, Filter oFilter, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)
        {
            m_type = type;
            m_dStartDate = dStartDate;
            m_dEndDate = dEndDate;
            m_nAssetIDs = nMediaIDs;
        }

        protected override void CheckRequestValidness()
        {
            if (m_nAssetIDs == null || m_nAssetIDs.Count == 0)
                throw new ArgumentException("No asset ids provided.");
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            AssetStatsResponse response = new AssetStatsResponse();

            try
            {
                CheckRequestValidness();
                CheckSignature(this);
                
                response.m_lAssetStat = Catalog.GetAssetStatsResults(m_nGroupID, m_nAssetIDs.Distinct<int>().ToList<int>(), m_dStartDate, m_dEndDate, m_type);
                response.m_nTotalItems = response.m_lAssetStat.Count;
              
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Exception", String.Concat("Req: ", ToString(), " Ex Msg: ", ex.Message, " Ex Type: ", ex.GetType().Name, " ST: ", ex.StackTrace), "AssetStatsRequest");
                throw ex;
            }

            return response;
        }
    }
}
