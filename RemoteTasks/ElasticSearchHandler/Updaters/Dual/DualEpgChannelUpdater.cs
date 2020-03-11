using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.Updaters
{
    public class DualEpgChannelUpdater : DualUpdater
    {
        public DualEpgChannelUpdater(int groupId, string urlV1, string urlV2)
        {
            oldUpdater = new EpgChannelUpdaterV1(groupId)
            {
                ElasticSearchUrl = urlV1
            };
            newUpdater = new EpgChannelUpdaterV2(groupId)
            {
                ElasticSearchUrl = urlV2
            };
        }
    }
}
