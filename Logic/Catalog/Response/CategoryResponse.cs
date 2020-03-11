using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Core.Catalog.Response
{
    [DataContract]
    public class CategoryResponse : BaseResponse
    {
        [DataMember]
        public Int32 ID;

        [DataMember]
        public string m_sTitle;

        [DataMember]
        public int m_nParentCategoryID;

        [DataMember]
        public string m_sCoGuid;

        [DataMember]
        public List<CategoryResponse> m_oChildCategories;

        [DataMember]
        public List<channelObj> m_oChannels;

        [DataMember]
        public List<Picture> m_lPics;

        public CategoryResponse()
        {
            m_oChildCategories = new List<CategoryResponse>();
            m_oChannels = new List<channelObj>();
        }
    }

}
