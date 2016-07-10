using ApiObjects.Response;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ApiObjects
{
    public class IngestAssetStatus
    {
        [DataMember]
        public Status Status { get; set; }

        [DataMember]
        public string ExternalAssetId { get; set; }

        [DataMember]
        public int InternalAssetId { get; set; }

        [DataMember]
        public List<Status> Warnings { get; set; }
    }
}
