using ElasticSearch.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.IndexBuilders
{
    public abstract class AbstractIndexBuilder
    {
        #region Data Members
        
        protected int groupId;
        protected ESSerializer serializer;
        protected ElasticSearchApi api;

        #endregion

        #region Properties

        public bool SwitchIndexAlias
        {
            get;
            set;
        }

        public bool DeleteOldIndices
        {
            get;
            set;
        }

        public DateTime? StartDate
        {
            get;
            set;
        }

        public DateTime? EndDate
        {
            get;
            set;
        }
        
        #endregion
        
        #region Ctor

        public AbstractIndexBuilder(int groupID)
        {
            this.groupId = groupID;
            api = new ElasticSearchApi();
            serializer = new ESSerializer();
        }

        #endregion

        #region Abstract Methods

        public abstract bool BuildIndex();

        #endregion

    }
}
