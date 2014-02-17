using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Catalog
{
    [DataContract]
    public class CommentsListResponse : BaseResponse
    {
        [DataMember]
        public List<Comments> m_lComments;

        public CommentsListResponse()
        {
            m_lComments = new List<Comments>();
        }
    }


    [DataContract]
    public class Comments
    {
        [DataMember]
        public Int32 Id;
        [DataMember]
        public Int32 m_nAssetID;
        [DataMember]
        public string m_sWriter;
        [DataMember]
        public int m_nLang;
        [DataMember]
        public string m_sLangName;
        [DataMember]
        public string m_sHeader;
        [DataMember]
        public string m_sSubHeader;
        [DataMember]
        public string m_sContentText;
        [DataMember]
        public DateTime m_dCreateDate;
        [DataMember]
        public string m_sSiteGuid;
        [DataMember]
        public string m_sUserPicURL;


        public Comments()
        {
        }
    }
}
