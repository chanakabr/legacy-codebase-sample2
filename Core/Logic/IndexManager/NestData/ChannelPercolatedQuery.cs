using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiLogic.IndexManager.NestData
{
    public class ChannelPercolatedQuery
    {
        [Percolator()]
        public QueryContainer Query { get; set; }
        
        [PropertyName("channel_id")]
        public int ChannelId { get; set; }
    }
}
