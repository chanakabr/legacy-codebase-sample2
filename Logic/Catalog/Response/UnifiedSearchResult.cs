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

    }

    [DataContract]
    public class RecordingSearchResult : UnifiedSearchResult
    {
        [DataMember]
        public string EpgId
        {
            get;
            set;
        }

        [DataMember]
        public RecordingType? RecordingType { get; set; }

        public RecordingSearchResult()
            : base() { }

        public RecordingSearchResult(ExtendedSearchResult extendedSearchResult)
            : base()
        {
            AssetId = string.Empty;
            EpgId = extendedSearchResult.AssetId;
            m_dUpdateDate = extendedSearchResult.m_dUpdateDate;
            AssetType = eAssetTypes.NPVR;
        }
    }

}
