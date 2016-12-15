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


        public RestNotificationEventConsumer()
        {
        }

        public override bool ShouldConsume(KalturaEvent kalturaEvent)
        {
            return true;
        }

        protected override bool Consume(KalturaEvent kalturaEvent)
        {
            bool result = false;
            WebAPI.Managers.Models.EventNotification specificNotification = null;
            WebAPI.Managers.Models.EventNotification generalNotification = null;

            Dictionary<string, NotificationAction> actions = new Dictionary<string,NotificationAction>();

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

                string cbKey = GetCBSpecificKey(kalturaEvent);
                specificNotification = cbManager.Get<EventNotification>(cbKey, true);
            }

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE)
            {
                Database = CB_SECTION_NAME,
                QueryType = KLogEnums.eDBQueryType.SELECT
            })
            {
                string cbKey = GetCBGeneralKey(kalturaEvent);
                generalNotification = cbManager.Get<EventNotification>(cbKey, true);
            }

            // chek if we have the metadata at all. if we don't we don't continue
            if (specificNotification == null && generalNotification == null)
            {
                shouldConsume = false;
            }
            else
            {
                // check if this feature is enabled/disabled for a specific group
                if ((generalNotification != null && generalNotification.Status != 1) ||
                    (specificNotification != null && specificNotification.Status != 1))
                {
                    shouldConsume = false;
                }
                // check if this event has any actions defined at all
                else 
                {
                    if (generalNotification != null && generalNotification.Actions != null)
                    {
                        foreach (var action in generalNotification.Actions)
                        {
                            actions[action.SystemName] = action;
                        }
                    }

                    if (specificNotification != null && specificNotification.Actions != null)
                    {
                        foreach (var action in specificNotification.Actions)
                        {
                            actions[action.SystemName] = action;
                        }
                    }

                    if (actions.Count == 0)
                    {
                        shouldConsume = false;
                    }
                }
            }

            if (!shouldConsume)
            {
                return false;
            }

            if (generalNotification != null)
            {
                Type source = kalturaEvent.Object.GetType();
            
                foreach (var action in actions.Values)
                {
                    try
                    {
                        // first check action's status
                        if (action.Status != 1)
                        {
                            continue;
                        }

                        // Get the type from the general definition
                        Type destination = Type.GetType(specificNotification.PhoenixType);

                        object t = AutoMapper.Mapper.Map(kalturaEvent.Object, source, destination);

                        object actionBody = action.Handler;
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
                            handler.Handle(kalturaEvent, t);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat(
                            "Error when performing action event for partner {0}, event type {1}, event action {2}, specific notification is {3}. ex = {4}",
                            kalturaEvent.PartnerId, kalturaEvent.Type, kalturaEvent.Action, action.ActionType, ex);
                    }
                }

                result = true;
            }

            return result;
        }

        protected string GetCBGeneralKey(KalturaEvent kalturaEvent)
        {
            return string.Format(CB_GENERAL_KEY_FORMAT, kalturaEvent.Type, kalturaEvent.Action).ToLower();
        }


        protected string GetCBSpecificKey(KalturaEvent kalturaEvent)
        {
            return string.Format(CB_SPECIFIC_PARTNER_KEY_FORMAT, kalturaEvent.PartnerId, kalturaEvent.Type, kalturaEvent.Action).ToLower();
        }
    }
}