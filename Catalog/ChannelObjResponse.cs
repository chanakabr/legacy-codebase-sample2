using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog
{
    [DataContract]
    public class ChannelObjResponse : BaseResponse
    {
        [DataMember]
        public Channel ChannelObj { get; set; }

        public ChannelObjResponse()
        {
            this.m_lObj = new List<BaseObject>();
        }
    }
}
