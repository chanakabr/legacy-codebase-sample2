using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.Updaters
{
    public class DualMediaUpdater : DualUpdater
    {
        public DualMediaUpdater(int groupId, string urlV1, string urlV2)
        {
            oldUpdater = new MediaUpdaterV1(groupId)
            {
                ElasticSearchUrl = urlV1,
                
            };
            newUpdater = new MediaUpdaterV2(groupId)
            {
                ElasticSearchUrl = urlV2
            };
        }
    }
}
