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
     /**************************************************************************
     * Get Personal Last Watched
     * Get SiteGuid
     * and return all the :
     * Last month medias that have been watched by SiteGuid
     * ************************************************************************/
    [DataContract]
    public class PersonalLastWatchedRequest : BaseRequest , IRequestImp
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PersonalLastWatchedRequest() : base()
        {
        }

        public PersonalLastWatchedRequest(PersonalLastWatchedRequest p)
            : base(p.m_nPageSize, p.m_nPageIndex, p.m_sUserIP, p.m_nGroupID, p.m_oFilter, p.m_sSignature, p.m_sSignString)
        {
            m_sSiteGuid = p.m_sSiteGuid;         
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                PersonalLastWatchedRequest request = (PersonalLastWatchedRequest)oBaseRequest;
                MediaIdsResponse response = new MediaIdsResponse();
                SearchResult oMediaObj;

                string xmlresult = "";
                xmlresult = SerializeToXML<PersonalLastWatchedRequest>(request);

                _logger.Info(xmlresult);
                _logger.Info(string.Format("{0}: {1}", "PersonalLastWatchedRequest Start At", DateTime.Now));

                if (request == null)
                    throw new Exception("request object is null or Required variables is null");

                string sCheckSignature = Utils.GetSignature(request.m_sSignString, request.m_nGroupID);
                if (sCheckSignature != request.m_sSignature)
                    throw new Exception("Signatures dosen't match");

                DataTable dt = CatalogDAL.Get_PersonalLastWatched(request.m_nGroupID, request.m_sSiteGuid);
                if (dt != null)
                {
                    if (dt.Columns != null)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            oMediaObj = new SearchResult();
                            oMediaObj.assetID = Utils.GetIntSafeVal(dt.Rows[i],"id");
                            if (!string.IsNullOrEmpty(dt.Rows[i]["Update_Date"].ToString()))
                            {
                                oMediaObj.UpdateDate = System.Convert.ToDateTime(dt.Rows[i]["Update_Date"].ToString());
                            }
                            response.m_nMediaIds.Add(oMediaObj);
                        }
                    }
                }

                response.m_nTotalItems = response.m_nMediaIds.Count;
                response.m_nMediaIds = Utils.GetMediaForPaging(response.m_nMediaIds, request);

                xmlresult = "no resultes";
                if (response != null)
                {
                    xmlresult = SerializeToXML<MediaIdsResponse>(response);
                }
                _logger.Info(xmlresult);
                return (BaseResponse)response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw ex;
            }
        }

    }
}
