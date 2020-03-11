using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EngagementAdapter.Models
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/EngagementAdapterCommon.Models")]
    public class EngagementResponse
    {
        [DataMember]
        public AdapterStatus Status { get; set; }

        [DataMember]
        public List<int> UserIds { get; set; }

        public EngagementResponse()
        {
            this.UserIds = new List<int>();
            this.Status = new AdapterStatus();
        }
    }
}