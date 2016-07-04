using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.Updaters
{
    public abstract class DualUpdater : IElasticSearchUpdater
    {
        protected IElasticSearchUpdater newUpdater;
        protected IElasticSearchUpdater oldUpdater;

        #region IElasticSearchUpdater Members

        public List<int> IDs
        {
            get
            {
                if (newUpdater != null)
                {
                    return newUpdater.IDs;
                }

                if (oldUpdater != null)
                {

                    return oldUpdater.IDs;
                }

                return null;
            }
            set
            {
                if (oldUpdater != null)
                {
                    oldUpdater.IDs = value;
                }

                if (newUpdater != null)
                {
                    newUpdater.IDs = value;
                }
            }
        }

        public ApiObjects.eAction Action
        {
            get
            {
                if (newUpdater != null)
                {
                    return newUpdater.Action;
                }

                if (oldUpdater != null)
                {

                    return oldUpdater.Action;
                }

                return ApiObjects.eAction.Update;
            }
            set
            {
                if (oldUpdater != null)
                {
                    oldUpdater.Action = value;
                }

                if (newUpdater != null)
                {
                    newUpdater.Action = value;
                }
            }
        }

        public bool Start()
        {
            bool oldSuccess = oldUpdater.Start();
            bool newSuccess = newUpdater.Start();

            bool success = oldSuccess && newSuccess;
            return success;
        }

        public string ElasticSearchUrl
        {
            get
            {
                if (newUpdater != null)
                {
                    return newUpdater.ElasticSearchUrl;
                }

                if (oldUpdater != null)
                {

                    return oldUpdater.ElasticSearchUrl;
                }

                return null;
            }
            set
            {
                if (oldUpdater != null)
                {
                    oldUpdater.ElasticSearchUrl = value;
                }

                if (newUpdater != null)
                {
                    newUpdater.ElasticSearchUrl = value;
                }
            }
        }

        #endregion
    }
}
