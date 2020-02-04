using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using AdapaterCommon.Models;

namespace REAdapter.Models
{
    [DataContract]
    public class RecommendationsResult
    {
        [DataMember]
        public AdapterStatus Status
        {
            get;
            set;
        }

        [DataMember]
        public SearchResult[] Results
        {
            get;
            set;
        }

        [DataMember]
        public string RequestId
        {
            get;
            set;
        }

        [DataMember]
        public int TotalResults
        {
            get;
            set;
        }
    }
}