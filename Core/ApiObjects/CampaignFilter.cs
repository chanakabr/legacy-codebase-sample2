using ApiObjects.Base;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiObjects
{
    public class CampaignFilter : ICrudFilter
    {
    }

    public class CampaignIdInFilter : CampaignFilter
    {
        public List<long> IdIn { get; set; }
    }

    public class CampaignSearchFilter : CampaignFilter
    {
        public long? StartDateGreaterThanOrEqual { get; set; }
        public long? EndDateLessThanOrEqual { get; set; }
        public long? EndDateGreaterThanOrEqual { get; set; }
        public ObjectState? StateEqual { get; set; }
        public bool? HasPromotion { get; set; }
    }

    public class TriggerCampaignFilter : CampaignSearchFilter
    {
        public ApiService? Service { get; set; }
        public ApiAction? Action { get; set; }
    }

    public class BatchCampaignFilter : CampaignSearchFilter
    {
    }
}