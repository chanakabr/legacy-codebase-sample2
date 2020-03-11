using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Core.Catalog.Cache;
using Core.Catalog.Response;
using GroupsCacheManager;
using KLogMonitor;
using Tvinci.Core.DAL;

namespace Core.Catalog.Request
{
    [DataContract]
    public class EpgCommentsListRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public Int32 m_nEpgProgramID;

        public EpgCommentsListRequest()
            : base()
        {
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                EpgCommentsListRequest request  = (EpgCommentsListRequest)oBaseRequest;
                CommentsListResponse   response = new CommentsListResponse();
                Comments comment;
                int pageIndex = request.m_nPageIndex;
                int pageSize = request.m_nPageSize;

                if (request == null)
                    throw new Exception("request object is null or Required variables is null");

                string sCheckSignature = Utils.GetSignature(request.m_sSignString, request.m_nGroupID);
                if (sCheckSignature != request.m_sSignature)
                    throw new Exception("Signatures dosen't match");

                int nLanguage = 0;
                if (request.m_oFilter != null)
                    nLanguage = request.m_oFilter.m_nLanguage;

                GroupManager groupManager = new GroupManager();
                CatalogCache catalogCache = CatalogCache.Instance();
                int nParentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);
                List<int> lSubGroup = groupManager.GetSubGroup(nParentGroupID);

                DataSet ds = CatalogDAL.Get_EPGCommentsList(request.m_nEpgProgramID, request.m_nGroupID, nLanguage, lSubGroup);

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
                        int countComment = 0;
                        int startIndex = pageIndex*pageSize;
                        if (pageIndex == 0 && pageSize == 0)
                            countComment = ds.Tables[1].Rows.Count;
                        else
                            countComment = startIndex + pageSize;


                        int count = Math.Min(countComment, ds.Tables[1].Rows.Count) ;
                        for (int i = startIndex; i < count; i++)
                        {
                            comment = new Comments();
                            comment.Id                = Utils.GetIntSafeVal(ds.Tables[1].Rows[i], "ID");
                            comment.m_nAssetID        = Utils.GetIntSafeVal(ds.Tables[1].Rows[i], "EPG_PROGRAM_ID");
                            comment.m_sWriter         = Utils.GetStrSafeVal(ds.Tables[1].Rows[i], "WRITER");
                            comment.m_nLang           = Utils.GetIntSafeVal(ds.Tables[1].Rows[i], "language_id");
                            comment.m_sLangName       = Utils.GetStrSafeVal(ds.Tables[1].Rows[i], "lang_name");
                            comment.m_sHeader         = Utils.GetStrSafeVal(ds.Tables[1].Rows[i], "HEADER");
                            comment.m_sSubHeader      = Utils.GetStrSafeVal(ds.Tables[1].Rows[i], "SUB_HEADER");
                            comment.m_sContentText    = Utils.GetStrSafeVal(ds.Tables[1].Rows[i], "CONTENT_TEXT");
                            comment.m_sSiteGuid       = Utils.GetStrSafeVal(ds.Tables[1].Rows[i], "SITE_GUID");
                            comment.m_sUserPicURL     = Utils.GetStrSafeVal(ds.Tables[1].Rows[i], "FACEBOOK_IMAGE");
                            if (!string.IsNullOrEmpty(ds.Tables[1].Rows[i]["create_date"].ToString()))
                            {
                                comment.m_dCreateDate = System.Convert.ToDateTime(ds.Tables[1].Rows[i]["create_date"].ToString());
                            }

                            if (!string.IsNullOrEmpty(pattern))
                            {
                                comment.m_sHeader = regex.Replace(comment.m_sHeader, "****");
                                comment.m_sSubHeader = regex.Replace(comment.m_sSubHeader, "****");
                                comment.m_sContentText = regex.Replace(comment.m_sContentText, "****");
                            }

                            comment.AssetType = ApiObjects.eAssetType.PROGRAM;
                            response.m_lComments.Add(comment);
                        }
                        response.m_nTotalItems = ds.Tables[1].Rows.Count;
                    }
                }
                return (BaseResponse)response;

            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
