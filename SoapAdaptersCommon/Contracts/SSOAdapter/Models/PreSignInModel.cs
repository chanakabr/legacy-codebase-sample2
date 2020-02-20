using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SSOAdapter.Models
{
    [DataContract]
    public class PreSignInModel
    {
        [DataMember]
        public int UserId { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public int MaxFailCount { get; set; }
        [DataMember]
        public int LockMin { get; set; }
        [DataMember]
        public int GroupId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
        [DataMember]
        public string IPAddress { get; set; }
        [DataMember]
        public string DeviceId { get; set; }
        [DataMember]
        public bool PreventDoubleLogin { get; set; }
        [DataMember]
        public IDictionary<string, string> CustomParams { get; set; }

    }
}