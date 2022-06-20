using System;
using ApiLogic.IndexManager.Helpers;
using Nest;

namespace ApiLogic.IndexManager.NestData
{
    public class NestLiveToVodProperties
    {
        [PropertyName(NamingHelper.EPG_CHANNEL_ID)]
        public long EpgChannelId { get; set; }

        [PropertyName(NamingHelper.LINEAR_ASSET_ID)]
        public long LinearAssetId { get; set; }

        [PropertyName(NamingHelper.CRID)]
        public string Crid { get; set; }

        [PropertyName(NamingHelper.EPG_ID)]
        public string EpgId { get; set; }

        [PropertyName(NamingHelper.ORIGINAL_START_DATE)]
        public DateTime OriginalStartDate { get; set; }

        [PropertyName(NamingHelper.ORIGINAL_END_DATE)]
        public DateTime OriginalEndDate { get; set; }
    }
}