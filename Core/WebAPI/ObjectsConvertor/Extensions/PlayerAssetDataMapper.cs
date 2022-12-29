using System;
using System.Collections.Generic;
using System.Text;
using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class PlayerAssetDataMapper
    {
        internal static int getLocation(this KalturaPlayerAssetData model)
        {
            return model.location.HasValue ? (int)model.location : 0;
        }

        internal static int getAverageBitRate(this KalturaPlayerAssetData model)
        {
            return model.averageBitRate.HasValue ? (int)model.averageBitRate : 0;
        }

        internal static int getCurrentBitRate(this KalturaPlayerAssetData model)
        {
            return model.currentBitRate.HasValue ? (int)model.currentBitRate : 0;
        }

        internal static int getTotalBitRate(this KalturaPlayerAssetData model)
        {
            return model.totalBitRate.HasValue ? (int)model.totalBitRate : 0;
        }
    }
}
