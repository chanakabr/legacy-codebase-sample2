using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog.Response
{
    [DataContract]
    public class ChannelViewsResponse : BaseResponse
    {
        [DataMember]
        public List<ChannelViewsResult> ChannelViews { get; set; }

        public ChannelViewsResponse()
        {
            this.ChannelViews = new List<ChannelViewsResult>();
        }
    }
}
