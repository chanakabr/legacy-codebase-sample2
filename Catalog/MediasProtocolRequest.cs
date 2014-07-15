using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Logger;
using System.Reflection;


namespace Catalog
{   
    [DataContract]
    public class MediasProtocolRequest : BaseRequest, IMediasProtocolRequest
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [DataMember]
        public List<Int32> m_lMediasIds;
      
        public MediasProtocolRequest(Int32 nPageSize, Int32 nPageIndex,string sUserIP, Int32 nGroupID, Filter oFilter, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)
        {
        }
        public MediasProtocolRequest() 
            : base()
        {          
        }

        /*Get Media Details By MediasIds*/
        public MediaResponse GetMediasByIDs(MediasProtocolRequest mediaRequest)
        {
            MediaResponse mediaResponse = new MediaResponse();
            List<MediaObj> lMediaObj = new List<MediaObj>();
            MediaObj oMediaObj = new MediaObj();

            _logger.Info(string.Format("{0}: {1}", "Catalog.GetMediasByIDs Start At", DateTime.Now));
            try
            {
                if (mediaRequest == null || mediaRequest.m_lMediasIds == null || mediaRequest.m_lMediasIds.Count == 0 || mediaRequest.m_oFilter == null)
                    throw new Exception("request object is null or Required variables is null");

                _logger.Info(string.Format("{0}: {1}", "count of MediasIDs", mediaRequest.m_lMediasIds.Count));

                string sCheckSignature = Utils.GetSignature(mediaRequest.m_sSignString, mediaRequest.m_nGroupID);
                if (sCheckSignature != mediaRequest.m_sSignature)             
                    throw new Exception("Signatures dosen't match");
                
                _logger.Info(string.Format("Start Complete Details for {0} MediaIds", mediaRequest.m_lMediasIds.Count));

                bool completeDetails = Catalog.CompleteDetailsForMediaResponse(mediaRequest, ref mediaResponse, mediaRequest.m_nPageSize * mediaRequest.m_nPageIndex, mediaRequest.m_nPageSize * mediaRequest.m_nPageIndex + mediaRequest.m_nPageSize);

                return mediaResponse;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
