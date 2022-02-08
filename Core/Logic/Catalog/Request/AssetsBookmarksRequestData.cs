using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog.Request
{
    [DataContract]
    public class AssetsBookmarksRequestData
    {
        [DataMember]
        public List<AssetBookmarkRequest> Assets;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("AssetsPositionRequestData object - ");
            foreach (AssetBookmarkRequest asset in Assets)
            {
                sb.Append(asset.ToString() + " ");
            }
            return sb.ToString();
        }
    }

    [DataContract]
    public class AssetBookmarkRequest
    {
        [DataMember]
        public eAssetTypes AssetType;

        [DataMember]
        public string AssetID;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("AssetLastPositionInfo object - ");
            sb.Append(string.Concat("AssetType :", AssetType));
            sb.Append(string.Concat("AssetID :", AssetID));            
            return sb.ToString();
        }
    }
    
    public sealed class AssetBookmarkRequestEqualityComparer : IEqualityComparer<AssetBookmarkRequest>
    {
        public bool Equals(AssetBookmarkRequest x, AssetBookmarkRequest y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.AssetType == y.AssetType && x.AssetID == y.AssetID;
        }

        public int GetHashCode(AssetBookmarkRequest obj)
        {
            unchecked
            {
                return ((int)obj.AssetType * 397) ^ (obj.AssetID != null ? obj.AssetID.GetHashCode() : 0);
            }
        }
    }

}