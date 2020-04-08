using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SSOAdapter.Models
{
    [DataContract]
    public partial class SSOAdapterProfileInvokeModel
    {
        [DataMember]
        public string Intent { get; set; }

        [DataMember]
        public IDictionary<string, string> ExtraParameters { get; set; }
    }
}
