using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog.Response
{
    [DataContract]
    public class IsMediaExistsInSubscriptionResponse : BaseResponse
    {
        [DataMember]
        public bool m_bExists { get; set; }

        public IsMediaExistsInSubscriptionResponse()
            : base()
        {
            m_bExists = false;
        }
    }
}
