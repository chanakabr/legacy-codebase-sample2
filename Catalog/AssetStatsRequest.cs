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
            AssetStatsRequest request = (AssetStatsRequest)oBaseRequest;
            AssetStatsResponse response = new AssetStatsResponse();

            try
            {
                if (request == null)
                    throw new Exception("request object is null");

                string sCheckSignature = Utils.GetSignature(request.m_sSignString, request.m_nGroupID);
                if (sCheckSignature != request.m_sSignature)
                    throw new Exception("Signatures do not match");

                //get the results
                response.m_lAssetStat = Catalog.GetAssetStatsResults(request.m_nGroupID, request.m_nAssetIDs, request.m_dStartDate, request.m_dEndDate, request.m_type);                    
              
            }
            catch (Exception ex)
            {
                BaseLog log = new BaseLog(eLogType.WcfRequest, DateTime.UtcNow, true);
                log.Method = "MediaStatsRequest GetResponse";                
                log.Message = string.Format("Could not retrieve the media Statistics from Catalog.GetMediaStatsResults. the response will be null. exception message: {0}, stack: {1}", ex.Message, ex.StackTrace);                                
                log.Error(log.Message, false);
 
                //previous log format
                Logger.Logger.Log("Error", "Could not retrieve the media Statistics from Catalog.GetMediaStatsResults. exception message: {0}, stack: {1}", ex.Message, ex.StackTrace, "Catalog");                
                response = null; 
            }

            return (BaseResponse)response;
        }
    }
}
