using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.EventNotifications
{
    public class RabbitQueueHandler : NotificationEventHandler
    {
        RabbitQueueDefinitions definitions = null;

        public RabbitQueueHandler(JObject definitionsJson)
            : base(definitionsJson)
        {
            definitions = definitionsJson.ToObject<RabbitQueueDefinitions>();
        }

        internal override void HandleEvent(EventManager.KalturaEvent kalturaEvent, object t)
        {
            //
        }
    }

    [Serializable]
    public class RabbitQueueDefinitions
    {
        [JsonProperty("routing_key")]
        public string RoutingKey
        {
            get;
            set;
        }
    }
}