using System.Collections.Generic;

namespace ApiObjects.AssetSelection
{
    public interface IAssetSelectionClient
    {
        IReadOnlyCollection<AssetSelectionItem> GetUserAssetSelections(int groupId, long userId, int slotNum);
    }
}