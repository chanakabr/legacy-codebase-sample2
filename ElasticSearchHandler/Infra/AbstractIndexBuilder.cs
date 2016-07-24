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
        protected ElasticSearchApi api;
        protected BaseESSeralizer serializer;

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
                else
                {
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
        }

        #endregion

        #region Abstract Methods

        public abstract bool BuildIndex();

        #endregion

        #region Protected Methods

        protected bool DualBuild(AbstractIndexBuilder firstBuilder, AbstractIndexBuilder secondBuilder)
        {
            bool success = false;

            // Copy definitions from current builder to the partial builders
            firstBuilder.SwitchIndexAlias = this.SwitchIndexAlias;
            secondBuilder.SwitchIndexAlias = this.SwitchIndexAlias;
            firstBuilder.DeleteOldIndices = this.DeleteOldIndices;
            secondBuilder.DeleteOldIndices = this.DeleteOldIndices;
            firstBuilder.StartDate = this.StartDate;
            secondBuilder.StartDate = this.StartDate;
            firstBuilder.EndDate = this.EndDate;
            secondBuilder.EndDate = this.EndDate;

            // Build the two indexes
            bool oldSuccess = firstBuilder.BuildIndex();
            bool newSuccess = secondBuilder.BuildIndex();

            success = oldSuccess && newSuccess;

            return success;
        }

        #endregion
    }
}
