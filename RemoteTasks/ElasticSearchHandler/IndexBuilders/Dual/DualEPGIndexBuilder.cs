using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.IndexBuilders
{
    public class DualEPGIndexBuilder : AbstractIndexBuilder
    {
        private EpgIndexBuilderV1 oldBuilder;
        private EpgIndexBuilderV2 newBuilder;

        public DualEPGIndexBuilder(int groupId, string urlV1, string urlV2)
            : base(groupId)
        {
            oldBuilder = new EpgIndexBuilderV1(groupId)
            {
                ElasticSearchUrl = urlV1
            };
            newBuilder = new EpgIndexBuilderV2(groupId)
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
