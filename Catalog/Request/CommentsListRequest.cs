using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data;
using System.Text.RegularExpressions;
using Tvinci.Core.DAL;
using Catalog.Response;
using KLogMonitor;

namespace Catalog.Request
{
    /**************************************************************************
    * Get Comments List
    * return all the :
    * Comments by media id 
    * ************************************************************************/
    [DataContract]
    public class CommentsListRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public Int32 m_nMediaID;

        public CommentsListRequest()
            : base()
        {
        }

        public CommentsListRequest(Int32 nMediaID, Int32 nCommentType, Int32 nGroupID, Int32 nPageSize, Int32 nPageIndex, string sUserIP, Filter oFilter, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)
        {
            m_nMediaID = nMediaID;
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                CommentsListRequest request = oBaseRequest as CommentsListRequest;
                CommentsListResponse response = new CommentsListResponse();
                Comments comment;

                if (request == null)
                    throw new ArgumentException("request object is null or Required variables is null");

                CheckSignature(request);

                int nLanguage = 0;
                if (request.m_oFilter != null)
                    nLanguage = request.m_oFilter.m_nLanguage;

                DataSet ds = CatalogDAL.Get_CommentsList(request.m_nMediaID, request.m_nGroupID, nLanguage);

                if (ds != null && ds.Tables.Count > 0)
                {
                    string pattern = string.Empty;
                    Regex regex = null;
                    if (ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                    {
                        pattern = Utils.GetStrSafeVal(ds.Tables[0].Rows[0], "Pattern");
                        regex = new Regex(pattern, RegexOptions.IgnoreCase);
                    }
                    if (ds.Tables[1].Columns != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                        {
                            comment = new Comments();
                            comment.Id                = Utils.GetIntSafeVal(ds.Tables[1].Rows[i],"ID");
                            comment.m_nAssetID        = Utils.GetIntSafeVal(ds.Tables[1].Rows[i],"MEDIA_ID");
                            comment.m_sWriter         =  Utils.GetStrSafeVal(ds.Tables[1].Rows[i],"WRITER");
                            comment.m_nLang           = Utils.GetIntSafeVal(ds.Tables[1].Rows[i],"language_id");
                            comment.m_sLangName       = Utils.GetStrSafeVal(ds.Tables[1].Rows[i],"lang_name");
                            comment.m_sHeader         = Utils.GetStrSafeVal(ds.Tables[1].Rows[i],"HEADER");
                            comment.m_sSubHeader      = Utils.GetStrSafeVal(ds.Tables[1].Rows[i],"SUB_HEADER");
                            comment.m_sContentText    = Utils.GetStrSafeVal(ds.Tables[1].Rows[i],"CONTENT_TEXT");
                            comment.m_sSiteGuid       = Utils.GetStrSafeVal(ds.Tables[1].Rows[i], "SITE_GUID");
                            comment.m_sUserPicURL = Utils.GetStrSafeVal(ds.Tables[1].Rows[i], "FACEBOOK_IMAGE");
                            if (!string.IsNullOrEmpty(ds.Tables[1].Rows[i]["create_date"].ToString()))
                            {
                                comment.m_dCreateDate = System.Convert.ToDateTime(ds.Tables[1].Rows[i]["create_date"].ToString());
                            }

                            if (!string.IsNullOrEmpty(pattern))
                            {
                                comment.m_sHeader = regex.Replace(comment.m_sHeader, "****");
                                comment.m_sContentText = regex.Replace(comment.m_sContentText, "****");
                            }

                            comment.AssetType = ApiObjects.eAssetType.MEDIA;
                            response.m_lComments.Add(comment);
                        }
                        response.m_nTotalItems = response.m_lComments.Count;
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
