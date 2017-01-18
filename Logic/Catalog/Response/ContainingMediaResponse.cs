using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog.Response
{
    [DataContract]
    public class ContainingMediaResponse : BaseResponse
    {
        [DataMember]
        public bool m_bContainsMedia;


        public ContainingMediaResponse()
        {
            m_bContainsMedia = false;
        } 
    }
}
