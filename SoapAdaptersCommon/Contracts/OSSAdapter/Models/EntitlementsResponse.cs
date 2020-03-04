using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using AdapaterCommon.Models;

namespace OSSAdapter.Models
{
    [DataContract]
    public class EntitlementsResponse
    {
        [DataMember]
        public List<Entitlement> Entitlements { get; set; }

        [DataMember]
        public AdapterStatus Status { get; set; }
    }
}