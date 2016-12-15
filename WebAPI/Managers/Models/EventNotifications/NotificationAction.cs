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

        [JsonProperty("body")]
        public object Body
        {
            get;
            set;
        }

    }
}