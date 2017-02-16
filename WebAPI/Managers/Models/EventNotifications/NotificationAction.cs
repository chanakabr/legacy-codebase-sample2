using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Managers.Models
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class NotificationAction
    {

        [JsonProperty("status")]
        public int Status
        {
            get;
            set;
        }

        [JsonProperty("system_name")]
        public string SystemName
        {
            get;
            set;
        }

        [JsonProperty("friendly_name")]
        public string FriendlyName
        {
            get;
            set;
        }

        [JsonProperty("conditions")]
        public List<NotificationCondition> Conditions
        {
            get;
            set;
        }

        [JsonProperty("is_asynch")]
        public bool IsAsync
        {
            get;
            set;
        }

        internal abstract void Handle(EventManager.KalturaEvent kalturaEvent, object t);
    }
}