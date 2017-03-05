using KLogMonitor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearch.Searcher
{
    public enum eElasticAggregationType
    {
        avg,
        cardinality,
        extended_stats,
        geo_bounds,
        geo_centroid,
        max,
        min,
        percentiles,
        stats,
        sum,
        value_count,
        terms,
        filter,
        filters
    }

    #region Aggregations

    public class ESBaseAggsItem
    {
        #region Members
        
        public string Name;
        public eElasticAggregationType Type;
        public List<ESBaseAggsItem> SubAggrgations;
        public string Meta;

        public string Field;
        List<KeyValuePair<string, string>> AdditionalInnerParameters;

        public bool IsNumeric;
        public int Size;
        public int? ShardSize;

        public string Order;
        public string OrderDirection;

        #endregion

        #region Ctor

        public ESBaseAggsItem()
        {
            this.Size = 0;
            this.ShardSize = null;
            this.SubAggrgations = new List<ESBaseAggsItem>();
            this.AdditionalInnerParameters = new List<KeyValuePair<string, string>>();
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            string result = string.Empty;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("\"{0}\":", this.Name);
            sb.Append("{");

            // META
            if (!string.IsNullOrEmpty(this.Meta))
            {
                sb.Append("\"meta\":{");
                sb.Append(this.Meta);
                sb.Append("},");
            }

            // AGGREGATIONS (sub aggregations)
            if (this.SubAggrgations != null && this.SubAggrgations.Count > 0)
            {
                sb.Append("\"aggregations\":{");

                foreach (ESBaseAggsItem item in this.SubAggrgations)
                {
                    sb.AppendFormat("{0},", item.ToString());
                }

                sb.Remove(sb.Length - 1, 1);
                sb.Append("},");
            }

            // Inner part - the actual aggregation part...
            string innerPart = this.InnerToString();

            sb.Append(innerPart);
            sb.Append("}");

            result = sb.ToString();

            return result;
        }

        #endregion

        #region Protected and Private Methods

        protected virtual string InnerToString()
        {
            string result = string.Empty;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("\"{0}\":", this.Type);
            sb.Append("{");
            sb.AppendFormat("\"field\": \"{0}\"", this.Field);

            if (!string.IsNullOrEmpty(this.Order))
            {

                if (string.IsNullOrEmpty(this.OrderDirection))
                {
                    this.OrderDirection = "desc";
                }

                sb.Append(",\"order\": { \"");
                sb.AppendFormat("{0} \" : \"{1}\"", this.Order, this.OrderDirection);
                sb.Append("}");
            }

            if (this.Size > -1 && this.IsSizeable())
            {
                sb.AppendFormat(",\"size\": {0}", this.Size);
            }

            if (this.ShardSize != null && this.ShardSize.HasValue)
            {
                sb.AppendFormat(",\"shard_size\": {0}", this.ShardSize.Value);
            }

            if (this.AdditionalInnerParameters != null && this.AdditionalInnerParameters.Count > 0)
            {
                foreach (var item in this.AdditionalInnerParameters)
                {
                    sb.AppendFormat(",\"{0}\": \"{1}\"", item.Key, item.Value);
                }
            }

            sb.Append("}");

            result = sb.ToString();

            return result;
        }

        /// <summary>
        /// Decides whether the "size" field should be defined in query or not
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsSizeable()
        {
            bool result = true;

            switch (this.Type)
            {
                case eElasticAggregationType.avg:
                case eElasticAggregationType.cardinality:
                case eElasticAggregationType.geo_bounds:
                case eElasticAggregationType.geo_centroid:
                case eElasticAggregationType.max:
                case eElasticAggregationType.extended_stats:
                case eElasticAggregationType.min:
                case eElasticAggregationType.percentiles:
                case eElasticAggregationType.stats:
                case eElasticAggregationType.sum:
                case eElasticAggregationType.value_count:
                {
                    result = false;
                    break;
                }
                case eElasticAggregationType.terms:
                case eElasticAggregationType.filter:
                case eElasticAggregationType.filters:
                {
                    result = true;
                    break;
                }
                default:
                break;
            }

            return result;
        }
        #endregion
    }

    public class ESFilterAggregation : ESBaseAggsItem
    {
        public IESTerm Filter;

        public ESFilterAggregation(IESTerm filter = null) : base()
        {
            this.Filter = filter;
            this.Type = eElasticAggregationType.filter;
        }

        protected override string InnerToString()
        {
            string result = string.Empty;

            StringBuilder sb = new StringBuilder();
            
            // FILTER
            if (this.Filter != null && !this.Filter.IsEmpty())
            {
                sb.AppendFormat("\"{0}\":", this.Type);

                sb.Append(this.Filter.ToString());
            }
            
            result = sb.ToString();

            return result;
        }
    }

    #endregion

}
