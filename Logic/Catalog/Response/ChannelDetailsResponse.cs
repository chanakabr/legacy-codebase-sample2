using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Core.Catalog.Response
{
    [DataContract]
    public class ChannelDetailsResponse : BaseResponse
    {
         [DataMember]
         public List<channelObj> m_lchannelList;


         public ChannelDetailsResponse()
        {
            m_lchannelList = new List<channelObj>();
        }
    }



     [DataContract]
     public class channelObj
     {
         [DataMember]
         public int m_nChannelID;
         [DataMember]
         public int m_nGroupID;
         [DataMember]
         public string m_sTitle;
         [DataMember]
         public string m_sDescription;
         [DataMember]
         public string m_sEditorRemarks;
         [DataMember]
         public DateTime m_dLinearStartTime;
         [DataMember]
         public List<Picture> m_lPic;
        
         public channelObj()
         {
             m_lPic = new List<Picture>();
         }
			
     }
}
