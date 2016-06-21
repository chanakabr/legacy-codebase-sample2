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
            bool success = false;

            // Copy definitions from current builder to the partial builders
            this.oldBuilder.SwitchIndexAlias = this.SwitchIndexAlias;
            this.newBuilder.SwitchIndexAlias = this.SwitchIndexAlias;
            this.oldBuilder.DeleteOldIndices = this.DeleteOldIndices;
            this.newBuilder.DeleteOldIndices = this.DeleteOldIndices;
            this.oldBuilder.StartDate = this.StartDate;
            this.newBuilder.StartDate = this.StartDate;
            this.oldBuilder.EndDate = this.EndDate;
            this.newBuilder.EndDate = this.EndDate;
            
            // Build the two indexes
            bool oldSuccess = this.oldBuilder.BuildIndex();
            bool newSuccess = this.newBuilder.BuildIndex();

            success = oldSuccess && newSuccess;

            return success;
        }
    }
}
