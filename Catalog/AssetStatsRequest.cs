using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using Logger;
using ApiObjects;

namespace Catalog
{
    [DataContract]
    public class AssetStatsRequest : BaseRequest, IRequestImp
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            AssetStatsResponse response = new AssetStatsResponse();

            try
            {
                CheckSignature(this);

                response.m_lAssetStat = Catalog.GetAssetStatsResults(m_nGroupID, m_nAssetIDs, m_dStartDate, m_dEndDate, m_type);                    
              
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
