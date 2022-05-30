using System.Collections.Generic;

namespace LiveToVod.BOL
{
    public class LiveToVodFullConfiguration
    {
        public bool IsLiveToVodEnabled { get; set; }
        
        public int RetentionPeriodDays { get; set; }
        
        public string MetadataClassifier { get; set; }
        
        public List<LiveToVodLinearAssetConfiguration> LinearAssets { get; set; }
    }
}