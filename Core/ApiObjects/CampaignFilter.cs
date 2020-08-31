using ApiObjects.Base;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiObjects
{
    public class CampaignFilter : ICrudFilter
    {
        public List<long> IdIn { get; set; }
    }

    public class TriggerCampaignFilter : CampaignFilter
    {
        public ApiService? Service { get; set; }
        public ApiAction? Action { get; set; }
    }
}
