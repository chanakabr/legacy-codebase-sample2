using System.Runtime.Serialization;

namespace EngagementAdapter.Models
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/EngagementAdapterCommon.Models")]
    public class AdapterStatus
    {
        [DataMember]
        public int Code { get; set; }

        [DataMember]
        public string Message { get; set; }

        public AdapterStatus()
        {
        }

        public AdapterStatus(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}