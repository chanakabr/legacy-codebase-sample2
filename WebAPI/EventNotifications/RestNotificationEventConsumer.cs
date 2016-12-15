using EventManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Managers.Models;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using WebAPI.EventNotifications;
using System.Reflection;

namespace WebAPI
{
    public class RestNotificationEventConsumer : BaseEventConsumer
    {
        private const string CB_SECTION_NAME = "groups";
        private const string CB_SPECIFIC_PARTNER_KEY_FORMAT = "notification_{0}_{1}_{2}";
        private const string CB_GENERAL_KEY_FORMAT = "notification_{0}_{1}";

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        ConcurrentDictionary<KalturaEvent, PartnerEventNotification> partnerSpecificNotifications = null;
        ConcurrentDictionary<KalturaEvent, GeneralEventNotification> generalNotifications = null;

        public RestNotificationEventConsumer()
        {
            generalNotifications = new ConcurrentDictionary<KalturaEvent, GeneralEventNotification>();
            partnerSpecificNotifications = new ConcurrentDictionary<KalturaEvent, PartnerEventNotification>();
        }

        public override bool ShouldConsume(KalturaEvent kalturaEvent)
        {
            string s = Newtonsoft.Json.JsonConvert.SerializeObject(typeof(object));

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

                string cbKey = string.Format(CB_SPECIFIC_PARTNER_KEY_FORMAT, kalturaEvent.PartnerId, kalturaEvent.Type, kalturaEvent.Action).ToLower();
                specificNotification = cbManager.Get<PartnerEventNotification>(cbKey, true);
            }

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE)
            {
                Database = CB_SECTION_NAME,
                QueryType = KLogEnums.eDBQueryType.SELECT
            })
            {
                string cbKey = string.Format(CB_GENERAL_KEY_FORMAT, kalturaEvent.Type, kalturaEvent.Action).ToLower();
                generalNotification = cbManager.Get<GeneralEventNotification>(cbKey, true);
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
                if ((generalNotification != null && generalNotification.Status != 1) || 
                    (specificNotification != null && specificNotification.Status != 1))
                {
                    shouldConsume = false;
                }
                // check if this event has any actions defined at all
                else if ((specificNotification == null || specificNotification.Actions == null) && generalNotification.Actions == null)
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
                // Get the type from the general definition
                Type destination = Type.GetType(generalNotification.PhoenixType);

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
                    try
                    {
                        object actionBody = action.Body;
                        JObject jsonObject = actionBody as JObject;
                        NotificationEventHandler handler = null;

                        if (jsonObject != null)
                        {
                            handler = NotificationActionFactory.CreateEventHandler(action.ActionType, jsonObject);
                        }
                        else
                        {
                            handler = NotificationActionFactory.CreateEventHandler(action.ActionType, actionBody.ToString());
                        }

                        if (handler != null)
                        {
                            handler.HandleEvent(kalturaEvent, t);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat(
                            "Error when performing action event for partner {0}, event type {1}, event action {2}, specific notification is {3}. ex = {4}",
                            kalturaEvent.PartnerId, kalturaEvent.Type, kalturaEvent.Action, action.ActionType, ex);
                    }
                }

                generalNotifications.TryRemove(kalturaEvent, out generalNotification);
                partnerSpecificNotifications.TryRemove(kalturaEvent, out partnerSpecificNotification);

                result = true;
            }

            return result;
        }
    }
}