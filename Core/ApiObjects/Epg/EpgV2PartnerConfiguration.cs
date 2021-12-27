namespace ApiObjects.Epg
{
    public class EpgV2PartnerConfiguration
    {
        public int PastIndexCompactionStart { get; set; } = 0;
        public int FutureIndexCompactionStart { get; set; } = 7;
        public bool IsEpgV2Enabled { get; set; } = false;
        public bool IsIndexCompactionEnabled { get; set; } = false;
    }
}