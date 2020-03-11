using System.Runtime.Serialization;

namespace AdapaterCommon.Models
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PGAdapterCommon.Models")]
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