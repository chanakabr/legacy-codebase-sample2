using System;

namespace Core.Catalog
{
    public class LiveToVodAsset : MediaAsset
    {
        public LiveToVodAsset(MediaAsset mediaAsset) : base(mediaAsset)
        {
        }

        public LiveToVodAsset(LiveToVodAsset liveToVodAsset) : base(liveToVodAsset)
        {
            OriginalStartDate = liveToVodAsset.OriginalStartDate;
            OriginalEndDate = liveToVodAsset.OriginalEndDate;
            EpgId = liveToVodAsset.EpgId;
            EpgChannelId = liveToVodAsset.EpgChannelId;
            EpgIdentifier = liveToVodAsset.EpgIdentifier;
            Crid = liveToVodAsset.Crid;
            LinearAssetId = liveToVodAsset.LinearAssetId;
            PaddingBeforeProgramStarts = liveToVodAsset.PaddingBeforeProgramStarts;
            PaddingAfterProgramEnds = liveToVodAsset.PaddingAfterProgramEnds;
        }

        public LiveToVodAsset()
        {
        }

        public long LinearAssetId { get; set; }
        public long EpgId { get; set; }
        public long EpgChannelId { get; set; }
        public string EpgIdentifier { get; set; }
        public string Crid { get; set; }
        public DateTime OriginalStartDate { get; set; }
        public DateTime OriginalEndDate { get; set; }
        public long PaddingBeforeProgramStarts { get; set; }
        public long PaddingAfterProgramEnds { get; set; }
    }
}