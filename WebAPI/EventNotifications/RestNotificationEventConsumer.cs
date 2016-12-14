using EventManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Managers.Models;
using System.Collections.Concurrent;

namespace WebAPI
{
    public class RestNotificationEventConsumer : BaseEventConsumer
    {
        private const string CB_SECTION_NAME = "groups";
        private const string CB_SPECIFIC_PARTNER_KEY_FORMAT = "notification_{0}_{1}_{2}";
        private const string CB_GENERAL_KEY_FORMAT = "notification_{0}_{1}";

        ConcurrentDictionary<KalturaEvent, PartnerEventNotification> partnerSpecificNotifications = null;
        ConcurrentDictionary<KalturaEvent, GeneralEventNotification> generalNotifications = null;

        public RestNotificationEventConsumer()
        {
            generalNotifications = new ConcurrentDictionary<KalturaEvent, GeneralEventNotification>();
            partnerSpecificNotifications = new ConcurrentDictionary<KalturaEvent, PartnerEventNotification>();
        }

        public override bool ShouldConsume(KalturaEvent kalturaEvent)
        {
            WebAPI.Managers.Models.PartnerEventNotification specificNotification = null;
            WebAPI.Managers.Models.GeneralEventNotification generalNotification = null;

            bool shouldConsume = true;

            // get the metadata for this type of event

            CouchbaseManager.CouchbaseManager cbManager = null;

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE)
            {
                Database = CB_SECTION_NAME,
                QueryType = KLogEnums.eDBQueryType.SELECT
            })
            {
                cbManager = new CouchbaseManager.CouchbaseManager(CB_SECTION_NAME);
                specificNotification = cbManager.Get<PartnerEventNotification>(string.Format(CB_SPECIFIC_PARTNER_KEY_FORMAT,
                    kalturaEvent.PartnerId, kalturaEvent.Type, kalturaEvent.Action), true);

            }

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE)
            {
                Database = CB_SECTION_NAME,
                QueryType = KLogEnums.eDBQueryType.SELECT
            })
            {
                generalNotification = cbManager.Get<GeneralEventNotification>(string.Format(CB_GENERAL_KEY_FORMAT,
                    kalturaEvent.Type, kalturaEvent.Action), true);
            }

            // chek if we have the metadata at all. if we don't we don't continue
            if (specificNotification == null && generalNotification == null)
            {
                shouldConsume = false;
            }
            else
            {
                generalNotifications[kalturaEvent] = generalNotification;
                partnerSpecificNotifications[kalturaEvent] = specificNotification;

                // check if this feature is enabled/disabled for a specific group
                if (generalNotification.Status != 1 || specificNotification.Status != 1)
                {
                    shouldConsume = false;
                }
                // check if this event has any actions defined at all
                else if (specificNotification.Actions == null && generalNotification.Actions == null)
                {
                    shouldConsume = false;
                }
            }

            if (!shouldConsume)
            {
                generalNotifications.TryRemove(kalturaEvent, out generalNotification);
                partnerSpecificNotifications.TryRemove(kalturaEvent, out specificNotification);
            }

            return shouldConsume;
        }

        protected override bool Consume(KalturaEvent kalturaEvent)
        {
            bool result = false;

            WebAPI.Managers.Models.PartnerEventNotification partnerSpecificNotification = null;
            WebAPI.Managers.Models.GeneralEventNotification generalNotification = null;

            bool getResult = partnerSpecificNotifications.TryGetValue(kalturaEvent, out partnerSpecificNotification);
            getResult = generalNotifications.TryGetValue(kalturaEvent, out generalNotification);

            if (generalNotification != null)
            {
                Type source = kalturaEvent.Object.GetType();
                Type destination = generalNotification.PhoenixType;

                object t = AutoMapper.Mapper.Map(kalturaEvent.Object, source, destination);

                List<NotificationAction> actions = new List<NotificationAction>();

                if (generalNotification.Actions != null)
                {
                    actions.AddRange(generalNotification.Actions);
                }

                if (partnerSpecificNotification != null && partnerSpecificNotification.Actions != null)
                {
                    actions.AddRange(partnerSpecificNotification.Actions);
                }

                foreach (var action in actions)
                {
                    var handler = NotificationActionFactory.CreateEventHandler(action.ActionType, action.Body);
                    handler.HandleEvent(kalturaEvent, t);
                }

                generalNotifications.TryRemove(kalturaEvent, out generalNotification);
                partnerSpecificNotifications.TryRemove(kalturaEvent, out partnerSpecificNotification);

                result = true;
            }

            return result;
            ;
        }
    }
}