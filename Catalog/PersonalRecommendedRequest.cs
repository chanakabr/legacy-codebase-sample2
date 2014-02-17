using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Logger;
using System.Reflection;
using System.Data;
using Tvinci.Core.DAL;
using ApiObjects.SearchObjects;

namespace Catalog
{
    /*********************************************************************
     * Get Personal Recommended Request 
     * Get SiteGuid and return all the :
     * The last Media that this siteGuid watched OR
     * If there isn't any media return the top most viewd media that 
     * have been watched by siteGuids in 
     * this group of groupID
     * *******************************************************************/
    [DataContract]
    public class PersonalRecommendedRequest : BaseRequest, IRequestImp
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [DataMember]
        public string m_sSiteGuid;

        public PersonalRecommendedRequest() : base()
        {
        }

        public PersonalRecommendedRequest(PersonalRecommendedRequest p)
            : base(p.m_nPageSize, p.m_nPageIndex, p.m_sUserIP, p.m_nGroupID, p.m_oFilter, p.m_sSignature, p.m_sSignString)
        {
            m_sSiteGuid = p.m_sSiteGuid;         
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                PersonalRecommendedRequest request = (PersonalRecommendedRequest)oBaseRequest;
                MediaIdsResponse response = new MediaIdsResponse();
                SearchResult oMediaRes;
                List<SearchResult> lMedias = new List<SearchResult>();

                string xmlresult = "";
                xmlresult = SerializeToXML<PersonalRecommendedRequest>(request);

                _logger.Info(xmlresult);
                _logger.Info(string.Format("{0}: {1}", "PersonalRecommendedRequest Start At", DateTime.Now));

                if (request == null)
                    throw new Exception("request object is null or Required variables is null");

                string sCheckSignature = Utils.GetSignature(request.m_sSignString, request.m_nGroupID);
                if (sCheckSignature != request.m_sSignature)
                    throw new Exception("Signatures dosen't match");

                DataTable dt = CatalogDAL.Get_PersonalRecommended(request.m_nGroupID, request.m_sSiteGuid, request.m_nPageSize * request.m_nPageIndex + request.m_nPageSize);

                if (dt != null)
                {
                    if (dt.Columns != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        if (dt.Rows[0]["getRelated"].ToString() == "1") // Call GetSearchRelated
                        {
                            MediaRelatedRequest oMediaRelatedRequest = new MediaRelatedRequest();
                            oMediaRelatedRequest.m_nGroupID = request.m_nGroupID;
                            oMediaRelatedRequest.m_nMediaID = Utils.GetIntSafeVal(dt.Rows[0],"media_id");
                            oMediaRelatedRequest.m_nPageIndex = request.m_nPageIndex;
                            oMediaRelatedRequest.m_nPageSize = request.m_nPageSize;
                            oMediaRelatedRequest.m_sSignString = request.m_sSignString;
                            oMediaRelatedRequest.m_sSignature = request.m_sSignature;
                            oMediaRelatedRequest.m_sUserIP = request.m_sUserIP;
                            oMediaRelatedRequest.m_oFilter = request.m_oFilter;
                            BaseResponse oBaseResponse =  oMediaRelatedRequest.GetResponse((BaseRequest)oMediaRelatedRequest);
                            
                            return oBaseResponse;                           
                        }
                        else //Retun last most viewd items
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                oMediaRes = new SearchResult();
                                oMediaRes.assetID = Utils.GetIntSafeVal(dt.Rows[i],"media_id");
                                if (!string.IsNullOrEmpty(dt.Rows[i]["Update_Date"].ToString()))
                                {
                                    oMediaRes.UpdateDate = System.Convert.ToDateTime(dt.Rows[i]["Update_Date"].ToString());
                                }
                                lMedias.Add(oMediaRes);
                            }
                            response.m_nTotalItems = lMedias.Count;
                            response.m_nMediaIds = Utils.GetMediaForPaging(lMedias, request); 
                            
                            xmlresult = "no resultes";
                            if (response != null)
                            {
                                xmlresult = SerializeToXML<MediaIdsResponse>(response);
                            }
                            _logger.Info(xmlresult);
                            return (BaseResponse)response;
                        }
                    }
                }
                return  (BaseResponse)response;
               
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw ex;
            }
        }


    }
}
