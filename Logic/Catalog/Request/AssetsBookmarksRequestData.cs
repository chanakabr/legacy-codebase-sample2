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

}