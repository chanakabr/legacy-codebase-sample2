using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SSOAdapter.Models
{
    [DataContract]
    public class PostSignInModel
    {
        [DataMember]
        public IDictionary<string,string> CustomParams { get; set; }

        [DataMember]
        public User AuthenticatedUser { get; set; }

    }
}