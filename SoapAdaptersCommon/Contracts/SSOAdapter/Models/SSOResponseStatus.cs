using System.Runtime.Serialization;

namespace SSOAdapter.Models
{
    public class SSOResponseStatus
    {
        [DataMember]
        public eSSOUserResponseStatus ResponseStatus { get; set; }
        [DataMember]
        public int ExternalCode { get; set; }
        [DataMember]
        public string ExternalMessage { get; set; }
    }
}