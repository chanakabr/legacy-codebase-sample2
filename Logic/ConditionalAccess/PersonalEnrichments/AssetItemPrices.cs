
using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess
{
    public class AssetItemPrices 
    {
        public string AssetId;
        public eAssetTypes AssetType;
        public List<MediaFileItemPricesContainer> PriceContainers;
    }
}
