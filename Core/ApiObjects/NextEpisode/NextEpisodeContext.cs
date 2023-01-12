namespace ApiObjects.NextEpisode
{
    public class NextEpisodeContext
    {
        public int GroupId { get; set; }
        public long UserId { get; set; }
        public NotWatchedReturnStrategy NotWatchedReturnStrategy { get; set; }
        public WatchedAllReturnStrategy WatchedAllReturnStrategy { get; set; }
    }
}