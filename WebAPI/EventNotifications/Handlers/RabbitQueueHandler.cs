using ApiObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI.EventNotifications
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class RabbitQueueHandler : NotificationAction
    {
        public RabbitQueueHandler()
            : base()
        {
        }

        internal override void Handle(EventManager.KalturaEvent kalturaEvent, KalturaNotification theObject)
        {
            // create queue and data objects
            QueueWrapper.GenericCeleryQueue queue = new QueueWrapper.GenericCeleryQueue();
            BaseCeleryData data = new BaseCeleryData()
            {
                id = Guid.NewGuid().ToString(),
                task = this.Task,
                // primary args object is the notified object itself, it will be a complete json object
                args = new List<object>()
                {
                    theObject
                }
            };

            // add extra args if they exist
            if (ExtraArgs != null && data.args != null)
            {
                data.args.AddRange(ExtraArgs);
            }

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

        [JsonProperty("extra_args")]
        public List<object> ExtraArgs
        {
            get;
            set;
        }
    }
}