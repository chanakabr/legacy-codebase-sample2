using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects.Response;
using Catalog.Response;
using ApiObjects;


namespace Catalog.Response
{
    [DataContract]
    public class AssetsPositionResponse : BaseResponse
    {

        [DataMember]
        public List<AssetPositionsInfo> AssetsPositions;

        [DataMember]
        public Status Status;

        public AssetsPositionResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }
    }

    [DataContract]
    public class AssetPositionsInfo
    {
        [DataMember]
        public eAssetTypes AssetType;

        [DataMember]
        public string AssetID;

        [DataMember]
        public List<LastPosition> LastPositions;

        public AssetPositionsInfo()
        {
        }

        public AssetPositionsInfo(eAssetTypes assetType, string assetID, List<LastPosition> lastPositions)
        {
            AssetType = assetType;
            AssetID = assetID;
            LastPositions = lastPositions;
        }
    }

}