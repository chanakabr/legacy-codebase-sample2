using AdapaterCommon.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace REAdapter.Models
{
    public class SearchResult
    {
        [DataMember]
        public string AssetId;

        [DataMember]
        public eAssetTypes AssetType
        {
            get;
            set;
        }

        [DataMember]
        public List<KeyValue> TagsExtraData{ get; set; }
    }

    public enum eAssetTypes
    {
        UNKNOWN = -1,
        EPG = 0,
        NPVR = 1,
        MEDIA = 2
    }
}