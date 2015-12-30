using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog.Request
{
    [DataContract]
    public class AssetsPositionRequestData
    {
        [DataMember]
        public List<AssetPositionRequestInfo> Assets;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("AssetsPositionRequestData object - ");
            foreach (AssetPositionRequestInfo asset in Assets)
            {
                sb.Append(asset.ToString() + " ");
            }
            return sb.ToString();
        }
    }

    [DataContract]
    public class AssetPositionRequestInfo
    {
        [DataMember]
        public eAssetTypes AssetType;

        [DataMember]
        public string AssetID;

        [DataMember]
        public eUserType UserType;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("AssetLastPositionInfo object - ");
            sb.Append(string.Concat("AssetType :", AssetType));
            sb.Append(string.Concat("AssetID :", AssetID));
            sb.Append(string.Concat("UserType :", UserType));
            return sb.ToString();
        }
    }

}