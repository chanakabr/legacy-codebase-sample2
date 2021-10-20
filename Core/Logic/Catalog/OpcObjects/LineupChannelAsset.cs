namespace Core.Catalog
{
    public class LineupChannelAsset : LiveAsset
    {
        public int? LinearChannelNumber { get; set; }

        public LineupChannelAsset(LiveAsset liveAsset, int? linearChannelNumber)
            : base(liveAsset)
        {
            LinearChannelNumber = linearChannelNumber;
        }
    }
}