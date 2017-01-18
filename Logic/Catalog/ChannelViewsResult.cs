using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog
{
    [DataContract]
    public class ChannelViewsResult
    {
        [DataMember]
        public int ChannelId { get; set; }
        [DataMember]
        public int NumOfViews { get; set; }

        public ChannelViewsResult(int nChannelId, int nNumOfViews)
        {
            this.ChannelId = nChannelId;
            this.NumOfViews = nNumOfViews;
        }
    }
}
