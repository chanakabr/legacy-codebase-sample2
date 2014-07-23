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

            try
            {

                CheckRequestValidness(mediaRequest);

                CheckSignature(mediaRequest);

                Catalog.CompleteDetailsForMediaResponse(mediaRequest, ref mediaResponse, mediaRequest.m_nPageSize * mediaRequest.m_nPageIndex, mediaRequest.m_nPageSize * mediaRequest.m_nPageIndex + mediaRequest.m_nPageSize);

                return mediaResponse;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw ex;
            }
        }

        private void CheckRequestValidness(MediasProtocolRequest request)
        {
            if (request == null || request.m_lMediasIds == null || request.m_lMediasIds.Count == 0
                || request.m_oFilter == null)
            {
                throw new ArgumentException("At least one of the arguments is not valid");
            }
        }
    }
}
