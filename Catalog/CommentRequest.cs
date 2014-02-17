using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Logger;
using Tvinci.Core.DAL;

namespace Catalog
{
    [DataContract]
    abstract public class CommentRequest : BaseRequest, IRequestImp
    {
        protected static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [DataMember]
        public string m_sWriter;
        [DataMember]
        public string m_sHeader;
        [DataMember]
        public string m_sSubHeader;
        [DataMember]
        public string m_sContentText;
        [DataMember]
        public string m_sSiteGuid;
        [DataMember]
        public string m_sUDID;
        [DataMember]
        public string m_sCountry;
        [DataMember]
        public bool  m_bAutoActive;
        [DataMember]
        public Int32  m_nAssetID;
        

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            CommentResponse response = new CommentResponse();

            CommentRequest cr = (CommentRequest)oBaseRequest;

            if (cr == null)
                throw new Exception("request object is null or Required variables is null");

            string sCheckSignature = Utils.GetSignature(cr.m_sSignString, cr.m_nGroupID);
            if (sCheckSignature != cr.m_sSignature)
                throw new Exception("Signatures dosen't match");


            if (string.IsNullOrEmpty(cr.m_sCountry))
            {
                cr.m_sCountry = TVinciShared.WS_Utils.GetIP2CountryCode(cr.m_sUserIP);
            }


            bool bInsert = this.PostComment(oBaseRequest);

            response.eStatusComment = bInsert ? StatusComment.SUCCESS : StatusComment.FAIL;

            return (BaseResponse)response;
        }

        abstract protected bool PostComment(BaseRequest oBaseRequest);
    }
}
