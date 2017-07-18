using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.SearchObjects
{
    [DataContract]
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
    public class SearchAggregationGroupBy
    {
        /// <summary>
        /// List of fields to group
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public List<string> groupBy
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public AggregationOrder? groupByOrder
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public int topHitsCount
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public string distinctGroup
        {
            get;
            set;
        }
    }
}
