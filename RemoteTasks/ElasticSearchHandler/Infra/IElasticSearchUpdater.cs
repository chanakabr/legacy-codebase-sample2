using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearchHandler.Updaters
{
    public interface IElasticSearchUpdater
    {
        string ElasticSearchUrl
        {
            get;
            set;
        }

        List<int> IDs { get; set; }
        ApiObjects.eAction Action { get; set; }

        bool Start();
    }
}
