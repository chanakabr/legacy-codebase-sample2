using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;

namespace Catalog.Response
{
    [DataContract]
    public class AggregationsResult
    {
        [DataMember]
        public string field;

        [DataMember]
        public List<AggregationResult> results;
    }

    [DataContract]
    public class AggregationResult
    {
        [DataMember]
        public string value;
        [DataMember]
        public int count;
        [DataMember]
        public List<AggregationsResult> subs;
    }
}
