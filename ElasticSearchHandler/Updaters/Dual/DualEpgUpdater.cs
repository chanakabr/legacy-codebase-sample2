using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.Updaters
{
    public class DualEpgUpdater : IElasticSearchUpdater
    {
        EpgUpdaterV1 oldUpdater;
        EpgUpdaterV2 newUpdater;

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
