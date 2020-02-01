using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using AdapaterCommon.Models;

namespace SSOAdapter.Models
{
    [DataContract]
    public class UserResponse
    {
        [DataMember]
        public AdapterStatusCode AdapterStatus { get; set; }

        [DataMember]
        public User User { get; set; }

        [DataMember]
        public SSOResponseStatus SSOResponseStatus { get; set; }

    }
}