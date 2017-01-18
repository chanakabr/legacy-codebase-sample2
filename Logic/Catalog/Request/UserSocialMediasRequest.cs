using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data;
using Tvinci.Core.DAL;
using ApiObjects.SearchObjects;
using Core.Catalog.Response;
using KLogMonitor;

namespace Core.Catalog.Request
{
    /****************************************************
     * Return all medias a user like or post 
     * by siteGuid / SocialPlatform / SocialAction
    ****************************************************/
    [DataContract]
    public class UserSocialMediasRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public int m_nSocialAction;
        [DataMember]
        public int m_nSocialPlatform;

        public UserSocialMediasRequest() 
            : base()
        {
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                UserSocialMediasRequest request = oBaseRequest as UserSocialMediasRequest;
                MediaIdsResponse response = new MediaIdsResponse();
                SearchResult oMediaRes;
                List<SearchResult> lMedias = new List<SearchResult>();

                if (request == null)
                    throw new Exception("request object is null or Required variables is null");

                CheckSignature(request);

                DataTable dt = CatalogDAL.Get_UserSocialMedias(request.m_sSiteGuid, request.m_nSocialPlatform, request.m_nSocialAction);
                if (dt != null)
                {
                    if (dt.Columns != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            oMediaRes = new SearchResult();
                            oMediaRes.assetID = Utils.GetIntSafeVal(dt.Rows[i],"id");
                            if (!string.IsNullOrEmpty(dt.Rows[i]["Update_Date"].ToString()))
                            {
                                oMediaRes.UpdateDate = System.Convert.ToDateTime(dt.Rows[i]["Update_Date"].ToString());
                            }
                            lMedias.Add(oMediaRes);
                        }
                        response.m_nTotalItems = lMedias.Count;
                        response.m_nMediaIds = Utils.GetMediaForPaging(lMedias, request);

                        return response;
                    }
                }
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
