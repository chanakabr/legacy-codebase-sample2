using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using AdapaterCommon.Models;

namespace SSOAdapter.Models
{
    [DataContract]
    public class SSOImplementationsResponse
    {
        [DataMember]
        public AdapterStatusCode Status { get; set; }
        [DataMember]
        public IEnumerable<eSSOMethods> ImplementedMethods { get; set; }

        [DataMember]
        public bool SendWelcomeEmail { get; set; }

    }
}