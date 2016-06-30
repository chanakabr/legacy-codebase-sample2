using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.Updaters
{
    public class DualChannelUpdater : DualUpdater
    {
        public DualChannelUpdater(int groupId, string urlV1, string urlV2)
        {
            oldUpdater = new ChannelUpdaterV1(groupId)
            {
                ElasticSearchUrl = urlV1
            };
            newUpdater = new ChannelUpdaterV2(groupId)
            {
                ElasticSearchUrl = urlV2
            };
        }
    }
}
