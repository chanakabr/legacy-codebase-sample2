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
        private const string CB_KEY_FORMAT = "notification_{0}";

        WebAPI.Managers.Models.EventNotification notification = null;

        public override bool ShouldConsume(KalturaEvent kalturaEvent)
        {
            string className = kalturaEvent.GetType().Name;

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE)
            {
                Database = CB_SECTION_NAME,
                QueryType = KLogEnums.eDBQueryType.SELECT
            })
            {
                CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(CB_SECTION_NAME);
                notification = cbManager.Get<EventNotification>(string.Format(CB_KEY_FORMAT, className), true);
            }

            bool shouldConsume = false;

            if (kalturaEvent is NotificationEvent)
            {
                shouldConsume = true;
            }

            return shouldConsume;
        }

        protected override bool Consume(KalturaEvent kalturaEvent)
        {
            Type source = kalturaEvent.Object.GetType(); //;
            Type destination = notification.PhoenixType;

            object t = AutoMapper.Mapper.Map(kalturaEvent.Object, source, destination);

            return false;
        }
    }
}