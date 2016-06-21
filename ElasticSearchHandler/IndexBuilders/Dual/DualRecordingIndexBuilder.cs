using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.IndexBuilders
{
    public class DualRecordingIndexBuilder : AbstractIndexBuilder
        {
        private RecordingIndexBuilderV1 oldBuilder;
        private RecordingIndexBuilderV2 newBuilder;

        public DualRecordingIndexBuilder(int groupId, string urlV1, string urlV2)
            : base(groupId)
        {
            oldBuilder = new RecordingIndexBuilderV1(groupId)
            {
                ElasticSearchUrl = urlV1
            };
            newBuilder = new RecordingIndexBuilderV2(groupId)
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
