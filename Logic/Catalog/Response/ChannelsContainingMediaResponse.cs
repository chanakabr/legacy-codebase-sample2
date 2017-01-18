using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog.Response
{
    public class ChannelsContainingMediaResponse : BaseResponse
    {
        [DataMember]
        public List<int> m_lChannellList;

        public ChannelsContainingMediaResponse()
            : base()
        {
            m_lChannellList = new List<int>();
        }
    }
}
