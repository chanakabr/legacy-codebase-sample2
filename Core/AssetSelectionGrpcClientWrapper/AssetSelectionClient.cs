using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects;
using ApiObjects.AssetSelection;
using OTT.Service.AssetSelection;
using AssetSelectionItem = ApiObjects.AssetSelection.AssetSelectionItem;

namespace AssetSelectionGrpcClientWrapper
{
    public class AssetSelectionClient : IAssetSelectionClient
    {
        private readonly AssetSelection.AssetSelectionClient _client;

        public AssetSelectionClient(AssetSelection.AssetSelectionClient client)
        {
            _client = client;
        }

        public IReadOnlyCollection<AssetSelectionItem> GetUserAssetSelections(int groupId, long userId, int slotNum)
        {
            var response = _client.GetAssetSelection(new AssetSelectionRequest
            {
                PartnerId = groupId,
                UserId = userId,
                SlotNum = slotNum
            });

            return response.Items.Select(Map).ToList();
        }
        
        // don't want to connect TvinciShared, too many dependencies
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        private static AssetSelectionItem Map(OTT.Service.AssetSelection.AssetSelectionItem item)
        {
            eAssetTypes assetType;
            switch (item.AssetType)
            {
                case AssetType.Epg:
                    assetType = eAssetTypes.EPG;
                    break;
                case AssetType.Media:
                    assetType = eAssetTypes.MEDIA;
                    break;
                default:
                    assetType = eAssetTypes.UNKNOWN;
                    break;
            }
            
            return new AssetSelectionItem(item.AssetId, assetType, Epoch.AddSeconds(item.UpdateDate));
        }
    }
}