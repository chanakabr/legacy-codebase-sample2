using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.IndexBuilders
{
    public class DualMediaIndexBuilder : AbstractIndexBuilder
    {
        private MediaIndexBuilderV1 oldBuilder;
        private MediaIndexBuilderV2 newBuilder;

        public DualMediaIndexBuilder(int groupId, string urlV1, string urlV2)
            : base(groupId)
        {
            oldBuilder = new MediaIndexBuilderV1(groupId)
            {
                ElasticSearchUrl = urlV1
            };
            newBuilder = new MediaIndexBuilderV2(groupId)
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
