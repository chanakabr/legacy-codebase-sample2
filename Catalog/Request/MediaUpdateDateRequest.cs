using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Logger;
using System.Reflection;
using System.Data;
using ApiObjects.SearchObjects;
using Catalog.Response;

namespace Catalog.Request
{
    [DataContract]
    public class MediaUpdateDateRequest : BaseRequest, IRequestImp
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [DataMember]
        public List<int> m_lMediaIds;

        public MediaUpdateDateRequest() : base()
        {
        }

        /*Get Max UpdateDate per mediaId*/
        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                MediaUpdateDateRequest request = (MediaUpdateDateRequest)oBaseRequest;
                MediaIdsResponse oMediaResponse = new MediaIdsResponse();
                SearchResult oMediaRes = new SearchResult();

                if (request == null)
                    throw new Exception("request object is null or Required variables is null");

                //Check signature - security 
                string sCheckSignature = Utils.GetSignature(request.m_sSignString, request.m_nGroupID);
                if (sCheckSignature != request.m_sSignature)
                    throw new Exception("Signatures dosen't match");

                //Complete max updatedate per mediaId
                List<SearchResult> lMediaRes = GetMediaUpdateDate(request.m_lMediaIds, request.m_nGroupID);

                if (lMediaRes != null)
                {
                    oMediaResponse.m_nMediaIds = new List<SearchResult>(lMediaRes);
                    oMediaResponse.m_nTotalItems = lMediaRes.Count;
                }
                return (BaseResponse)oMediaResponse;
            }
            catch (Exception ex)
            {
                _logger.Error("MediaUpdateDateRequest.GetResponse", ex);
                throw ex;
            }
        }       
    }
}
