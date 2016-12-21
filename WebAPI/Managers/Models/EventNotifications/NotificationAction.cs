using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Managers.Models
{
    public enum eNotificationActionTypes
    {
        Http = 0,
        Email = 1,
        RabbitQueue = 2,
        // to be continued
    }

    [Serializable]
    public class NotificationAction
    {
        [JsonProperty("type")]
        public eNotificationActionTypes ActionType
        {
            get;
            set;
        }

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

        [JsonProperty("handler")]
        public object Handler
        {
            get;
            set;
        }
    }
}