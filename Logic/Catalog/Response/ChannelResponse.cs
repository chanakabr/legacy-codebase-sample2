using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using ApiObjects.SearchObjects;

namespace Core.Catalog.Response
{
  
    [DataContract]
    public class ChannelResponse : BaseResponse
    {
        [DataMember]
        public Int32 Id;
        [DataMember]
        public string m_sDescription;
        [DataMember]
        public int m_sEnableRssFeed;
        [DataMember]
        public string m_sName;
        [DataMember]
        public string m_sImageUrl;
        [DataMember]
        public List<SearchResult> m_nMedias;
       

        public ChannelResponse()
        {
            m_nMedias = new List<SearchResult>();
        }
    }


    //[DataContract]
    //public class MediaRes
    //{
    //    [DataMember]
    //    public int m_nMediaId;
    //    [DataMember]
    //    public DateTime m_dUpdateDate;
    //    public MediaRes()
    //    {
           
    //    }
    //}
}
