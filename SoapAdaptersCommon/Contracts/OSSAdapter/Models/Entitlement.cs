using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OSSAdapter.Models
{
    [DataContract]
    public class Entitlement
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public eTransactionType EntitlementType { get; set; }

        [DataMember]
        public string ContentId { get; set; }

        [DataMember]
        public long StartDateSeconds { get; set; }

        [DataMember]
        public long EndDateSeconds { get; set; }
    }
}