using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using TVinciShared;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using ApiObjects.SearchObjects;
using Core.Catalog.Response;
using KLogMonitor;

namespace Core.Catalog.Request
{
    /**************************************************************************************
   * return : Return all medias that share the same values Like the mediaID that was send
   * *************************************************************************************/
    [DataContract]
    public class MediaRelatedRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public Int32 m_nMediaID;

        [DataMember]
        public List<Int32> m_nMediaTypes;

        [DataMember]
        public string m_sFilter;

        [DataMember]
        public OrderObj OrderObj;

        public MediaRelatedRequest()
            : base()
        {
            m_nMediaTypes = new List<Int32>();
        }

        public MediaRelatedRequest(Int32 nMediaID, Int32 nGroupID, Int32 nPageSize, Int32 nPageIndex, string sUserIP, Filter oFilter, string sSignature, string sSignString, List<Int32> nMediaTypes, OrderObj orderObj)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)
        {
            m_nMediaID = nMediaID;
            m_nMediaTypes = nMediaTypes;
            m_nMediaTypes = new List<Int32>();
            OrderObj = orderObj;
        }

        public MediaRelatedRequest(MediaRelatedRequest m)
            : base(m.m_nPageSize, m.m_nPageIndex, m.m_sUserIP, m.m_nGroupID, m.m_oFilter, m.m_sSignature, m.m_sSignString)
        {
            m_nMediaID = m.m_nMediaID;
            m_nMediaTypes = m.m_nMediaTypes;
            m_nMediaTypes = new List<Int32>();
            OrderObj = m.OrderObj;
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            MediaRelatedRequest request = oBaseRequest as MediaRelatedRequest;
            UnifiedSearchResponse searchResponse = new UnifiedSearchResponse();

            Filter oFilter = new Filter();
            try
            {
                //Build  MediaSearchRequest object
                if (request == null || request.m_nMediaID == 0 || request.m_oFilter == null)
                    throw new ArgumentException("request object is null or Required variables is null");

                CheckSignature(request);
                
                searchResponse.status = CatalogLogic.GetRelatedAssets(request, out searchResponse.m_nTotalItems, out searchResponse.searchResults);
                return searchResponse;               
            }
            catch (Exception ex)
            {
                log.Error("GetMediaRelated", ex);
                throw ex;
            }
        }
    }
}
