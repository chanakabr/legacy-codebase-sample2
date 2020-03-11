using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Core.Catalog.Response
{
    [DataContract]
   
    public class PicResponse : BaseResponse
    {        
        public PicResponse()
        {
        }
    }
    
    [DataContract]
   
    public class PicObj : BaseObject
    {
        [DataMember]
        public List<Picture> m_Picture;


        public PicObj()
            : base()
        {
            m_Picture = new List<Picture>();
        }
    }
}
