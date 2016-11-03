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

                //Filter MediaIds according to pageSize and PageIndex
                List<int> filteredMediaIds = FilterMediaIds(request.m_lMediaIds, request.m_nPageIndex, request.m_nPageSize);

                //Complete max updatedate per mediaId
                List<SearchResult> lMediaRes = GetMediaUpdateDate(filteredMediaIds, request.m_nGroupID);

                if (lMediaRes != null)
                {
                    oMediaResponse.m_nMediaIds = lMediaRes;
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

        private List<int> FilterMediaIds(List<int> mediaIdsToFilter, int pageIndex, int pageSize)
        {
            List<int> filteredMediaIds = new List<int>();
            if (mediaIdsToFilter != null && mediaIdsToFilter.Count > 0)
            {
                filteredMediaIds = mediaIdsToFilter.OrderBy(x => x).ToList();
                int totalResults = filteredMediaIds.Count;
                int startIndexOnList = pageIndex * pageSize;
                int rangeToGetFromList = (startIndexOnList + pageSize) > totalResults ? (totalResults - startIndexOnList) > 0 ? (totalResults - startIndexOnList): 0 : pageSize;
                if (rangeToGetFromList > 0)
                {
                    filteredMediaIds = filteredMediaIds.GetRange(startIndexOnList, rangeToGetFromList);
                }
                else
                {
                    filteredMediaIds.Clear();
                }
            }

            return filteredMediaIds;
        }
    }
}
