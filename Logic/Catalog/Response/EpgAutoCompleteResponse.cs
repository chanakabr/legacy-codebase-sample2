using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog.Response
{
    [DataContract]
    public class EpgAutoCompleteResponse : BaseResponse
    {
        [DataMember]
        public List<string> m_sList;
        public EpgAutoCompleteResponse()
            : base()
        {
            m_sList = new List<string>();
        }
    }
}
