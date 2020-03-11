using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog.Response
{
    [DataContract]
    public class MediaChannelsResponse : BaseResponse
    {

        [DataMember]
        public List<int> m_nChannelIDs;

        public MediaChannelsResponse()
        {
            m_nChannelIDs = new List<int>();
        }
    }
}
