using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using AdapaterCommon.Models;

namespace SSOAdapter.Models
{
    [DataContract]
    public class PreSignInResponse
    {
        [DataMember]
        public AdapterStatusCode AdapterStatus { get; set; }
        
        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }
        
       
        [DataMember]
        public IDictionary<string, string> Priviliges { get; set; }

        [DataMember]
        public SSOResponseStatus SSOResponseStatus { get; set; }

    }
}