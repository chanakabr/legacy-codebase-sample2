using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.Updaters
{
    public class DualEpgChannelUpdater : IElasticSearchUpdater
    {
        EpgChannelUpdaterV1 oldUpdater;
        EpgChannelUpdaterV2 newUpdater;

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

        #region IElasticSearchUpdater Members

        public List<int> IDs
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ApiObjects.eAction Action
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Start()
        {
            throw new NotImplementedException();
        }

        public string ElasticSearchUrl
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
