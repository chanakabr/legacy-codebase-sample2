using ApiObjects.Response;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ApiObjects
{
    [DataContract(Namespace = "", Name = "Response")]
    public class IngestResponse
    {
        [DataMember(Name = "status", Order = 0)]
        public string Status { get; set; }

        [DataMember(Name = "description", Order = 1)]
        public string Description { get; set; }

        [DataMember(Name = "assetID", Order = 2)]
        public string AssetID { get; set; }

        [DataMember(Name = "tvmID", Order = 3)]
        public string TvmID { get; set; }

        [DataMember]
        public Status IngestStatus { get; set; }

        [DataMember(Name = "AssetsStatus", Order = 4)]
        public List<IngestAssetStatus> AssetsStatus { get; set; }
    }
}

