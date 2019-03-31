using System.Collections.Generic;

namespace ApiObjects
{
    public class IngestProfile
    {
        [DBFieldMapping("ID")]
        public int Id { get; set; }

        [DBFieldMapping("name")]
        public string Name { get; set; }

        [DBFieldMapping("external_identifier")]
        public string ExternalId { get; set; }

        [DBFieldMapping("asset_type")]
        public int AssetTypeId { get; set; }

        [DBFieldMapping("transformation_adapter_url")]
        public string TransformationAdapterUrl { get; set; }

        [DBFieldMapping("transformation_adapter_config")]
        public IList<IngestProfileAdapterParam> Settings { get; set; }

        [DBFieldMapping("transformation_adapter_shared_secret")]
        public string TransformationAdapterSharedSecret { get; set; }

        [DBFieldMapping("default_autofill_policy")]
        public int DefaultAutoFillPolicy { get; set; }

        [DBFieldMapping("default_overlap_policy")]
        public int DefaultOverlapPolicy { get; set; }
    }
}
