using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Logger;
using Tvinci.Core.DAL;

namespace Catalog.Request
{
   
    [DataContract]
    public class MediaCommentRequest : CommentRequest
    {
        public MediaCommentRequest()
            : base()
        {
        }

        override protected bool PostComment(BaseRequest oBaseRequest)
        {
            try
            {
                MediaCommentRequest request = (MediaCommentRequest)oBaseRequest;

                int nIsActive = request.m_bAutoActive == true ? 1 : 0;

                return CatalogDAL.InsertMediaComment(request.m_sSiteGuid, request.m_nAssetID, request.m_sWriter,
                    request.m_nGroupID, nIsActive, 0, request.m_sUserIP, request.m_sHeader,
                    request.m_sSubHeader, request.m_sContentText, request.m_oFilter.m_nLanguage, request.m_sUDID);
            }
            catch (Exception ex)
            {
                _logger.Error("MediaCommentRequest", ex);
                throw ex;
            }
        }
    };
}
