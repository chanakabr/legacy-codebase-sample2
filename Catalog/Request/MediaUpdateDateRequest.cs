using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data;
using ApiObjects.SearchObjects;
using Catalog.Response;
using KLogMonitor;

namespace Catalog.Request
{
    [DataContract]
    public class MediaUpdateDateRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
                    oMediaResponse.m_nMediaIds = FilterResult(lMediaRes, request.m_nPageIndex, request.m_nPageSize);
                    oMediaResponse.m_nTotalItems = lMediaRes.Count;
                }
                return (BaseResponse)oMediaResponse;
            }
            catch (Exception ex)
            {
                log.Error("MediaUpdateDateRequest.GetResponse", ex);
                throw ex;
            }
        }

        private List<SearchResult> FilterResult(List<SearchResult> mediasToFilter, int pageIndex, int pageSize)
        {
            List<SearchResult> filteredMedias = new List<SearchResult>();
            if (mediasToFilter != null && mediasToFilter.Count > 0)
            {
                filteredMedias = mediasToFilter.OrderBy(x => x.assetID).ToList();
                int totalResults = filteredMedias.Count;
                int startIndexOnList = pageIndex * pageSize;
                int rangeToGetFromList = (startIndexOnList + pageSize) > totalResults ? (totalResults - startIndexOnList) > 0 ? (totalResults - startIndexOnList) : 0 : pageSize;
                if (rangeToGetFromList > 0)
                {
                    filteredMedias = filteredMedias.GetRange(startIndexOnList, rangeToGetFromList);
                }
                else
                {
                    filteredMedias.Clear();
                }

            }

            return filteredMedias;
        }
        
    }
}
