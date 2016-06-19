using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using GroupsCacheManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchHandler.IndexBuilders
{
    public abstract class AbstractIndexBuilder
    {
        #region Data Members
        
        protected int groupId;
        protected ESSerializerV1 serializer;
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

        public string ElasticSearchUrl
        {
            get
            {
                if (api != null)
                {
                    return api.baseUrl;
                }
                else{
                    return null;
                }
            }
            set
            {
                if (api != null)
                {
                    api.baseUrl = value;
                }
            }
        }

        #endregion
        
        #region Ctor

        public AbstractIndexBuilder(int groupID)
        {
            this.groupId = groupID;
            api = new ElasticSearchApi();
            serializer = new ESSerializerV1();
        }

        #endregion

        #region Abstract Methods

        public abstract bool BuildIndex();

        #endregion

    }
}
