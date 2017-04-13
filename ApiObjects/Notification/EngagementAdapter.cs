using System.Collections.Generic;

namespace ApiObjects.Notification
{
    public class EngagementAdapter : EngagementAdapterBase
    {
        public bool IsActive { get; set; }
        public string AdapterUrl { get; set; }
        public string SharedSecret { get; set; }

        public bool SkipSettings { get; set; }
        public List<EngagementAdapterSettings> Settings { get; set; }

        public EngagementAdapter()
        {
            Settings = new List<EngagementAdapterSettings>();
        }
    }
}
