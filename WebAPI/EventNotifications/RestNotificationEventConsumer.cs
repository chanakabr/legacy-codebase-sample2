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
using ApiObjects;
using System.Threading.Tasks;

namespace WebAPI
{
    public class RestNotificationEventConsumer : BaseEventConsumer
    {
        #region Consts
        
        private const string CB_SECTION_NAME = "groups";
        private const string CB_SPECIFIC_PARTNER_KEY_FORMAT = "notification_{0}_{1}_{2}";
        private const string CB_GENERAL_KEY_FORMAT = "notification_{0}_{1}";

        #endregion

        #region Logger
        
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #endregion

        #region Ctor

        public RestNotificationEventConsumer()
        {
        }

        #endregion

        #region Override methods

        public override bool ShouldConsume(KalturaEvent kalturaEvent)
        {
            bool result = false;

            if (kalturaEvent is KalturaObjectActionEvent)
            {
                result = true;
            }

            return result;
        }

        protected override bool Consume(KalturaEvent kalturaEvent)
        {
            bool result = false;

            KalturaObjectActionEvent actionEvent = kalturaEvent as KalturaObjectActionEvent;
            EventNotification specificNotification = null;
            EventNotification generalNotification = null;
            Dictionary<string, NotificationAction> actions = new Dictionary<string, NotificationAction>();

            log.DebugFormat("Start consume Notification event for: partnerId = {0}, type = {1}, action = {2}",
                kalturaEvent.PartnerId, actionEvent.Type, actionEvent.Action);

            #region Get notification definitions

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

                string cbKey = GetCBSpecificKey(actionEvent);
                specificNotification = cbManager.Get<EventNotification>(cbKey, true);
            }

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE)
            {
                Database = CB_SECTION_NAME,
                QueryType = KLogEnums.eDBQueryType.SELECT
            })
            {
                string cbKey = GetCBGeneralKey(actionEvent);
                generalNotification = cbManager.Get<EventNotification>(cbKey, true);
            }

            // chek if we have the metadata at all. if we don't we don't continue
            if (specificNotification == null && generalNotification == null)
            {
                shouldConsume = false;
            }
            else
            {
                // first we take the general definition of the action, according to system's name
                if (generalNotification != null && generalNotification.Actions != null)
                {
                    foreach (var action in generalNotification.Actions)
                    {
                        actions[action.SystemName] = action;
                    }
                }

                // Then we override the general definition with the partner-specific definition
                if (specificNotification != null && specificNotification.Actions != null)
                {
                    foreach (var action in specificNotification.Actions)
                    {
                        actions[action.SystemName] = action;
                    }
                }

                // Check acount of actions
                if (actions.Count == 0)
                {
                    shouldConsume = false;
                }
            }

            if (!shouldConsume)
            {
                return false;
            }

            #endregion

            #region Convert object

            Type source = actionEvent.Object.GetType();

            string phoenixType = string.Empty;

            // by default - phoenix type is from general defintiion
            if (generalNotification != null && !string.IsNullOrEmpty(generalNotification.PhoenixType))
            {
                phoenixType = generalNotification.PhoenixType;
            }

            // partner-specific definition overrides it (if needed)
            if (specificNotification != null && !string.IsNullOrEmpty(specificNotification.PhoenixType))
            {
                phoenixType = specificNotification.PhoenixType;
            }

            // Get the phoenix type Type
            Type destination = Type.GetType(phoenixType);

            // convert the WS object to an API/rest/phoenixObject
            object phoenixObject = AutoMapper.Mapper.Map(actionEvent.Object, source, destination);

            #endregion

            #region Handle actions

            // Perform the actions for this event
            foreach (var action in actions.Values)
            {

                log.DebugFormat("Notification event action: action name = {0}", action.FriendlyName);
                try
                {
                    // first check action's status - if it isn't good,
                    if (action.Status != 1)
                    {
                        continue;
                    }
                    bool conditionResult = true;

                    // Check conditions for this action. If all are OK, perform action. If one fails, stop.
                    if (action.Conditions != null && action.Conditions.Count > 0)
                    {
                        foreach (var condition in action.Conditions)
                        {
                            // Only for active/enabled conditions
                            if (condition.Status == 1)
                            {
                                conditionResult &= condition.Evaluate(kalturaEvent, phoenixObject);
                            }
                        }
                    }

                    if (!conditionResult)
                    {
                        continue;
                    }

                    if (!action.IsAsync)
                    {
                        action.Handle(kalturaEvent, phoenixObject);
                    }
                    else
                    {
                        Task t = Task.Factory.StartNew(() =>
                            {
                                action.Handle(kalturaEvent, phoenixObject);
                            }
                            );
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat(
                        "Error when performing action event for partner {0}, event type {1}, event action {2}, specific notification is {3}. ex = {4}",
                        kalturaEvent.PartnerId, actionEvent.Type, actionEvent.Action, action.GetType().ToString(), ex);
                }
            }

            #endregion

            result = true;

            return result;
        }

        #endregion

        #region Protected methods

        protected string GetCBGeneralKey(KalturaObjectActionEvent kalturaEvent)
        {
            return string.Format(CB_GENERAL_KEY_FORMAT, kalturaEvent.Type, kalturaEvent.Action).ToLower();
        }

        protected string GetCBSpecificKey(KalturaObjectActionEvent kalturaEvent)
        {
            return string.Format(CB_SPECIFIC_PARTNER_KEY_FORMAT, kalturaEvent.PartnerId, kalturaEvent.Type, kalturaEvent.Action).ToLower();
        }

        #endregion

    }
}