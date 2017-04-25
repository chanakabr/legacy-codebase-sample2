using APILogic;
using ApiObjects;
using KLogMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Api.Modules
{
    [Serializable]
    [JsonObject()]
    public class SearchHistory : CoreObject
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Consts

        private const string KEY_FORMAT = "search_history_{0}_{1}";
        private static readonly string CB_SEARCH_HISTORY_DESIGN_DOC = ODBCWrapper.Utils.GetTcmConfigValue("search_history_design_doc");

        #endregion

        #region Properties

        [JsonProperty("documentId")]
        public string id
        {
            get;
            set;
        }

        [JsonProperty("groupId")]
        public int groupId
        {
            get;
            set;
        }

        [JsonProperty("type")]
        public string type
        {
            get;
            set;
        }

        [JsonProperty("name")]
        public string name
        {
            get;
            set;
        }

        [JsonProperty("language")]
        public string language
        {
            get;
            set;
        }

        [JsonProperty("createdAt")]
        public long createdAt
        {
            get;
            set;
        }
        
        [JsonIgnore()]
        public JObject filter
        {
            get;
            set;
        }

        [JsonProperty("filter")]
        public string filterKey
        {
            get;
            set;
        }
        
        [JsonProperty("service")]
        public string service
        {
            get;
            set;
        }

        [JsonProperty("action")]
        public string action
        {
            get;
            set;
        }

        [JsonProperty("userId")]
        public string userId
        {
            get;
            set;
        }

        [JsonProperty("deviceId")]
        public string deviceId
        {
            get;
            set;
        }

        #endregion

        public SearchHistory()
        {
            this.type = "searchHistory";
            id = Guid.NewGuid().ToString();
        }

        protected override bool DoInsert()
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.SOCIAL);

            if (this.language == null)
            {
                this.language = string.Empty;
            }

            // 3 months
            uint expiration = 60 * 60 * 24 * 90;

            this.id = string.Format(KEY_FORMAT, this.userId, this.createdAt);
            this.filterKey = Utils.CalculateMD5Hash(this.filter.ToString());

            result = couchbaseManager.Set(this.id, this, expiration, true);

            result &= couchbaseManager.Set(this.filterKey, filter, expiration, true);

            if (!result)
            {
                log.ErrorFormat("Failed inserting Search History object to Couchbase. id = {0}, name = {1}", id, name);
            }

            return result;
        }

        protected override bool DoUpdate()
        {
            throw new NotImplementedException();
        }

        protected override bool DoDelete()
        {
            throw new NotImplementedException();
        }

        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }

        internal static List<SearchHistory> List(int groupId, string userId, string udid, string language, int pageIndex, int? pageSize, out int totalItems)
        {
            List<SearchHistory> result = new List<SearchHistory>();
            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.SOCIAL);

            int skip = 0;
            int limit = 0;

            if (pageSize != null && pageSize.HasValue)
            {
                limit = pageSize.Value;
                skip = pageSize.Value * pageIndex;
            }

            if (language == null)
            {
                language = string.Empty;
            }

            long totalNumOfResults = 0;

            // 1. get the basic search history documents, without the filter
            result = couchbaseManager.View<SearchHistory>(new CouchbaseManager.ViewManager(CB_SEARCH_HISTORY_DESIGN_DOC, "user_search_history")
            {
                key = new object[]{userId, language},
                inclusiveEnd = true,
                fullSet = true,
                skip = skip,
                limit = limit
            }, 
            ref totalNumOfResults);

            List<string> filterKeys = new List<string>();
            Dictionary<string, List<SearchHistory>> filterKeyToSearchHistory = new Dictionary<string, List<SearchHistory>>();

            // 2. map the filter keys to the search history items
            foreach (var searchHistory in result)
            {
                filterKeys.Add(searchHistory.filterKey);

                if (!filterKeyToSearchHistory.ContainsKey(searchHistory.filterKey))
                {
                    filterKeyToSearchHistory[searchHistory.filterKey] = new List<SearchHistory>();
                }

                filterKeyToSearchHistory[searchHistory.filterKey].Add(searchHistory);
            }

            // 3. get from CB all the filter objects
            var filters = couchbaseManager.GetValues<object>(filterKeys);

            // 4. complete search history item with filter object
            foreach (var filter in filters)
            {
                string filterKey = filter.Key;

                foreach (var searchHistory in filterKeyToSearchHistory[filterKey])
                {
                    searchHistory.filter = JObject.Parse(filter.Value.ToString());
                }
            }

            totalItems = (int)totalNumOfResults;

            return result;
        }
    }
}
