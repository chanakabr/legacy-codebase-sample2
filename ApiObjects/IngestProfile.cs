using System.Collections.Generic;

namespace ApiObjects
{
    public class IngestProfile
    {
        [DBFieldMapping("ID")]
        public int Id { get; set; }

        [DBFieldMappingAttribute("group_id")]
        public int GroupId { get; set; }

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
        public eIngestProfileAutofillPolicy DefaultAutoFillPolicy { get; set; }

        [DBFieldMapping("default_overlap_policy")]
        public eIngestProfileOverlapPolicy DefaultOverlapPolicy { get; set; }

        [DBFieldMapping("overlap_channels")]
        public IList<int> OverlapChannels { get; set; }

        public override string ToString()
        {
            return $"{{ id:'{Id}', groupId:'{GroupId}', name:'{Name}', assetType:'{AssetTypeId}' }}";
        }
    }

    /// <summary>
    /// 0 - reject input with holes
    /// 1 - autofill holes
    /// 2 - keep holes and don’t autofill
    /// </summary>
    public enum eIngestProfileAutofillPolicy
    {
        Reject = 0,
        Autofill = 1,
        KeepHoles = 2
    }

    /// <summary>
    /// indicates how overlaps in EPG should be managed
    /// (a setting per liniar media id will also be avaiable)
    /// 0 - reject input with overlap
    /// 1 - cut source
    /// 2 - cut target
    /// </summary>
    public enum eIngestProfileOverlapPolicy
    {
        Reject = 0,
        CutSource = 1,
        CutTarget = 2
    }
}
