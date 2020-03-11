using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.Updaters
{
    public class DualEpgUpdater : DualUpdater
    {
        public DualEpgUpdater(int groupId, string urlV1, string urlV2)
        {
            oldUpdater = new EpgUpdaterV1(groupId)
            {
                ElasticSearchUrl = urlV1
            };
            newUpdater = new EpgUpdaterV2(groupId)
            {
                ElasticSearchUrl = urlV2
            };
        }
    }
}
