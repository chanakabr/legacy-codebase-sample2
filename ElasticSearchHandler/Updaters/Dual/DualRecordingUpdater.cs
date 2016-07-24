using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.Updaters
{
    public class DualRecordingUpdater : DualUpdater
    {
        public DualRecordingUpdater(int groupId, string urlV1, string urlV2)
        {
            oldUpdater = new RecordingUpdaterV1(groupId)
            {
                ElasticSearchUrl = urlV1
            };
            newUpdater = new RecordingUpdaterV2(groupId)
            {
                ElasticSearchUrl = urlV2
            };
        }
    }
}
