using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Managers.Models
{
    [Serializable]
    public class NotificationAction
    {
        [JsonProperty("type")]
        public string Type
        {
            get;
            set;
        }

        [JsonProperty("body")]
        public NotificationActionDefinitions Body
        {
            get;
            set;
        }

        public virtual bool Consume(EventManager.KalturaEvent kalturaEvent, object eventObject)
        {
            bool result = false;

            return result;
        }
    }
}