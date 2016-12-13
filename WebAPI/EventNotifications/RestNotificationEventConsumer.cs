using EventManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Managers.Models;

namespace WebAPI
{
    public class RestNotificationEventConsumer :  BaseEventConsumer
    {
        private const string CB_SECTION_NAME = "groups";
        private const string CB_KEY_FORMAT = "notification_{0}_{1}_{2}";

        WebAPI.Managers.Models.EventNotification notification = null;

        public override bool ShouldConsume(KalturaEvent kalturaEvent)
        {
            bool shouldConsume = true;

            // get the metadata for this type of event
            
            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE)
            {
                Database = CB_SECTION_NAME,
                QueryType = KLogEnums.eDBQueryType.SELECT
            })
            {
                CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(CB_SECTION_NAME);
                notification = cbManager.Get<EventNotification>(string.Format(CB_KEY_FORMAT, 
                    kalturaEvent.PartnerId, kalturaEvent.Type, kalturaEvent.Action), true);
            }

            // chek if we have the metadata at all. if we don't we don't continue
            if (notification == null)
            {
                shouldConsume = false;
            }
            else
            {
                // check if this feature is enabled/disabled for a specific group
                if (notification.Status != 1)
                {
                    shouldConsume = false;
                }
                else  if (notification.Actions == null)
                {
                    shouldConsume = false;
                }
            }

            return shouldConsume;
        }

        protected override bool Consume(KalturaEvent kalturaEvent)
        {
            Type source = kalturaEvent.Object.GetType();
            Type destination = notification.PhoenixType;

            object t = AutoMapper.Mapper.Map(kalturaEvent.Object, source, destination);
            
            foreach (var action in notification.Actions)
            {
                var handler = NotificationActionFactory.CreateEventHandler(action.ActionType, action.Body);
                handler.HandleEvent(kalturaEvent, t);
            }

            return false;
        }
    }
}