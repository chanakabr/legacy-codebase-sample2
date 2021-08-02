using System;
using Nest;

namespace ApiObjects.Nest
{
    public class NestSocialActionStatistics
    {
        [PropertyName("media_id")]
        public int MediaID { get; set; }
        
        [PropertyName("group_id")]
        public int GroupID { get; set; }
        
        [PropertyName("media_type")]
        public string MediaType { get; set; }
        
        [PropertyName("action_date")]
        public DateTime Date { get; set; }
        
        [PropertyName("action")]
        public string Action { get; set; }
        
        [PropertyName("rate_value")]
        public int RateValue { get; set; }

        [PropertyName("count")]
        public int? Count { get; set; }

        public NestSocialActionStatistics()
        {
            GroupID = 0;
            MediaType = string.Empty;
            Date = DateTime.UtcNow;
            Action = string.Empty;
            RateValue = 0;
        }
    }
}