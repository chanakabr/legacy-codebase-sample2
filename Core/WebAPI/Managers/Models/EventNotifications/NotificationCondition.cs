using EventManager;
using Newtonsoft.Json;
using System;

namespace WebAPI.Managers.Models
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class NotificationCondition
    {
        [JsonProperty("status")]
        public int Status
        {
            get;
            set;
        }

        public virtual bool Evaluate(KalturaEvent kalturaEvent, object eventObject)
        {
            return true;
        }
    }
}