using System;

namespace DAL.DTO
{
    public class LiveToVodAssetDTO
    {
        public long LinearAssetId { get; set; }
        public long EpgId { get; set; }
        public long EpgChannelId { get; set; }
        public string EpgIdentifier { get; set; }
        public string Crid { get; set; }
        public DateTime OriginalStartDate { get; set; }
        public DateTime OriginalEndDate { get; set; }
        public long PaddingBeforeProgramStarts { get; set; }
        public long PaddingAfterProgramEnds { get; set; }
        public long MediaId { get; set; }
        public long UpdaterId { get; set; }
    }
}