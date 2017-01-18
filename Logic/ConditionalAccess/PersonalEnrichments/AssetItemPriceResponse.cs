using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess.Response
{
    public class AssetItemPriceResponse
    {
        public ApiObjects.Response.Status Status;

        public List<AssetItemPrices> Prices;
    }
}
