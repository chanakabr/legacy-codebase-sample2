using System.Runtime.Serialization;

namespace Core.Catalog.CatalogManagement
{
    public class AssetFile
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public long AssetId { get; set; }

        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public double Duration { get; set; }

        [DataMember]
        public string ExternalId { get; set; }

        [DataMember]
        public string BillingType { get; set; }

        [DataMember]
        public string Quality { get; set; }
    }
}
