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
        [JsonProperty("id")]
        public string id
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

        //[JsonIgnore()]
        //public DateTime createDate
        //{
        //    get
        //    {
        //        return TVinciShared.DateUtils.UnixTimeStampToDateTime(this.createdAt);
        //    }
        //    set
        //    {
        //        this.createdAt = TVinciShared.DateUtils.DateTimeToUnixTimestamp(value);
        //    }
        //}

        [JsonProperty("filter")]
        public JObject filter
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

        public SearchHistory()
        {
            this.type = "searchHistory";
            id = Guid.NewGuid().ToString();
        }

        protected override bool DoInsert()
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager manager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.SOCIAL);

            // 3 months
            uint expiration = 60 * 60 * 24 * 90;

            result = manager.Add(this.id, this, expiration, true);

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
    }
}
