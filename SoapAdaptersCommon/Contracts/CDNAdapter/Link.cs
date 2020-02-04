using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace CDNAdapter.Models
{
    [DataContract]
    public class Link
    {
        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string ProviderStatusCode { get; set; }

        [DataMember]
        public string ProviderStatusMessage { get; set; }
    }
}