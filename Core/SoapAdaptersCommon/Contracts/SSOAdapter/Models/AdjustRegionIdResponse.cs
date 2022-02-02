using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using AdapaterCommon.Models;

namespace SSOAdapter.Models
{
    [DataContract]
    public class AdjustRegionIdResponse
    {
        [DataMember]
        public AdapterStatusCode AdapterStatus { get; set; }

        [DataMember]
        public int RegionId { get; set; }

        [DataMember]
        public SSOResponseStatus SSOResponseStatus { get; set; }

    }
}