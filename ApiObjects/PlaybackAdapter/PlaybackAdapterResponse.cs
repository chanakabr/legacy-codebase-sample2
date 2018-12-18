namespace ApiObjects.PlaybackAdapter
{
    public class PlaybackAdapterResponse
    {
        public Response.Status Status { get; set; }
        public AdapterPlaybackContext PlaybackContext { get; set; }
    }
}
