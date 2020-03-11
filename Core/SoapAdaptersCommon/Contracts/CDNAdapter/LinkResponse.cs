using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using AdapaterCommon.Models;

namespace CDNAdapter.Models
{
    public class LinkResponse
    {
        [DataMember]
        public AdapterStatus Status { get; set; }

        [DataMember]
        public Link Link { get; set; }
    }
}