using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.IndexBuilders
{
    public class DualChannelIndexBuilder : AbstractIndexBuilder
    {
        private ChannelIndexBuilderV1 oldBuilder;
        private ChannelIndexBuilderV2 newBuilder;

        public DualChannelIndexBuilder(int groupId, string urlV1, string urlV2)
            : base(groupId)
        {
            oldBuilder = new ChannelIndexBuilderV1(groupId)
            {
                ElasticSearchUrl = urlV1
            };
            newBuilder = new ChannelIndexBuilderV2(groupId)
            {
                ElasticSearchUrl = urlV2
            };
        }

        public override bool BuildIndex()
        {
            return DualBuild(this.oldBuilder, this.newBuilder);
        }
    }
}
