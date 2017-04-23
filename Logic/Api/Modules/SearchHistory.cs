using APILogic;
using ApiObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Api.Modules
{
    [Serializable]
    [JsonObject()]
    public class SearchHistory : CoreObject
    {
        #region Consts

        private const string KEY_FORMAT = "search_history_{0}_{1}";
        private static readonly string CB_SEARCH_HISTORY_DESIGN_DOC = ODBCWrapper.Utils.GetTcmConfigValue("search_history_design_doc");

        #endregion

        #region Properties

        [JsonProperty("id")]
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

            // 3 months
            uint expiration = 60 * 60 * 24 * 90;

            this.id = string.Format(KEY_FORMAT, this.userId, this.createdAt);
            this.filterKey = Utils.CalculateMD5Hash(this.filter.ToString());

            result = couchbaseManager.Set(this.id, this, expiration, true);

            result &= couchbaseManager.Set(this.filterKey, filter, expiration, true);

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

        internal static List<SearchHistory> List(int groupId, string userId, string udid, string language, int pageIndex, int? pageSize)
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

            result = couchbaseManager.View<SearchHistory>(new CouchbaseManager.ViewManager(CB_SEARCH_HISTORY_DESIGN_DOC, "user_search_history")
            {
                key = new object[]{userId, language},
                inclusiveEnd = true,
                fullSet = true,
                skip = skip,
                limit = limit
            });

            return result;
        }
    }
}
