using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Search assets request
    /// </summary>
    public class KalturaSearchAssetsRequest : KalturaBaseAssetsRequest
    {
        /// <summary>
        /// List of asset types to search within.
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "filter_types")]
        [JsonProperty(PropertyName = "filter_types")]
        public List<int> filter_types { get; set; }

        /// <summary>
        /// Search assets using dynamic criteria. Provided collection of nested expressions with key, comparison operators, value, and logical conjunction.
        /// Possible keys: any Tag or Meta defined in the system and the following reserved keys: start_date, end_date.
        /// Comparison operators: for numerical fields =, >, >=, <![CDATA[<]]>, <![CDATA[<=]]>. For alpha-numerical fields =, != (not), ~ (like), !~, ^ (starts with). Logical conjunction: and, or.
        /// (maximum length of 1024 characters)
        /// </summary>
        [DataMember(Name = "filter")]
        [JsonProperty(PropertyName = "filter")]
        public string filter { get; set; }

        /// <summary>
        /// Required sort option to apply for the identified assets. If omitted – will use relevancy.
        /// Possible values: relevancy, a_to_z, z_to_a, views, ratings, votes, newest.
        /// </summary>
        [DataMember(Name = "order_by")]
        [JsonProperty(PropertyName = "order_by")]
        public KalturaOrder? order_by { get; set; }

    }
}