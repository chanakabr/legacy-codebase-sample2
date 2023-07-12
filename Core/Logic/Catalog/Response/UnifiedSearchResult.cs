using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects;

namespace Core.Catalog.Response
{
    [DataContract]
    [KnownType(typeof(RecordingSearchResult))]
    public class UnifiedSearchResult : BaseObject
    {
        [DataMember]
        public double Score { get; set; }
    }

    [DataContract]
    public class RecordingSearchResult : UnifiedSearchResult
    {
        [DataMember]
        public string EpgId { get; set; }

        [DataMember]
        public RecordingType? RecordingType { get; set; }

        [DataMember]
        public string RecordingId { get; set; }

        [DataMember]
        public bool IsMulti { get; set; }

        public RecordingSearchResult() : base() { }

        public RecordingSearchResult(ExtendedSearchResult extendedSearchResult) : base()
        {
            AssetId = string.Empty;
            EpgId = extendedSearchResult.AssetId;
            m_dUpdateDate = extendedSearchResult.m_dUpdateDate;
            AssetType = eAssetTypes.NPVR;
        }

        public RecordingSearchResult(ExtendedRecordingSearchResult extendedSearchResult) : base()
        {
            AssetId = extendedSearchResult.AssetId;
            EpgId = extendedSearchResult.EpgId;
            m_dUpdateDate = extendedSearchResult.m_dUpdateDate;
            AssetType = eAssetTypes.NPVR;
            RecordingType = extendedSearchResult.RecordingType;
            IsMulti = extendedSearchResult.IsMulti;
        }
    }

    [DataContract]
    public class RecommendationSearchResult : UnifiedSearchResult
    {
        [DataMember]
        public List<KeyValuePair<string, string>> TagsExtraData { get; set; }

        public RecommendationSearchResult() : base() { }
    }

    [DataContract]
    public class EpgSearchResult : UnifiedSearchResult
    {
        public EpgSearchResult(ExtendedEpgSearchResult extendedSearchResult) : base()
        {
            AssetId = extendedSearchResult.AssetId;
            m_dUpdateDate = extendedSearchResult.m_dUpdateDate;
            AssetType = eAssetTypes.EPG;
            DocumentId = extendedSearchResult.DocumentId;
        }

        [DataMember]
        public string DocumentId { get; set; }

        public EpgSearchResult() : base() { }
    }
}
