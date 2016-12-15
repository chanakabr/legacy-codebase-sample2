using ApiObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.EventNotifications
{
    [Serializable]
    public class RabbitQueueHandler : NotificationEventHandler
    {
        public RabbitQueueHandler()
            : base()
        {
        }

        internal override void Handle(EventManager.KalturaEvent kalturaEvent, object theObject)
        {
            //
            QueueWrapper.GenericCeleryQueue queue = new QueueWrapper.GenericCeleryQueue();
            BaseCeleryData data = new BaseCeleryData()
            {
                id = Guid.NewGuid().ToString(),
                task = this.Task,
                args = new List<object>()
                {
                    theObject
                }
            };

            queue.Enqueue(data, this.RoutingKey, this.Expiration);
        }

        [JsonProperty("routing_key")]
        public string RoutingKey
        {
            get;
            set;
        }

        [JsonProperty("expiration")]
        public long Expiration
        {
            get;
            set;
        }

        [JsonProperty("task")]
        public string Task
        {
            get;
            set;
        }
    }
}