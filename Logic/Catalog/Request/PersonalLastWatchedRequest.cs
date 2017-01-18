using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data;
using Tvinci.Core.DAL;
using ApiObjects.SearchObjects;
using Core.Catalog.Cache;
using Core.Catalog.Response;
using KLogMonitor;

namespace Core.Catalog.Request
{
    /**************************************************************************
    * Get Personal Last Watched
    * Get SiteGuid
    * and return all the :
    * Last month medias that have been watched by SiteGuid
    * ************************************************************************/
    [DataContract]
    public class PersonalLastWatchedRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public PersonalLastWatchedRequest()
            : base()
        {
        }

        public PersonalLastWatchedRequest(PersonalLastWatchedRequest p)
            : base(p.m_nPageSize, p.m_nPageIndex, p.m_sUserIP, p.m_nGroupID, p.m_oFilter, p.m_sSignature, p.m_sSignString)
        {
            m_sSiteGuid = p.m_sSiteGuid;
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                PersonalLastWatchedRequest request = oBaseRequest as PersonalLastWatchedRequest;
                MediaIdsResponse response = new MediaIdsResponse();
                SearchResult oMediaObj;

                if (request == null)
                    throw new ArgumentNullException("request object is null or Required variables is null");

                CheckSignature(request);

                DataTable dt = CatalogDAL.Get_PersonalLastWatched(request.m_nGroupID, request.m_sSiteGuid);
                if (dt != null)
                {
                    if (dt.Columns != null)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            oMediaObj = new SearchResult();
                            oMediaObj.assetID = Utils.GetIntSafeVal(dt.Rows[i], "id");
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

                return response;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
