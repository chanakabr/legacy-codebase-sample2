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
using WebAPI.Models.General;
using System.Web.Hosting;
using KlogMonitorHelper;

namespace WebAPI
{
    public class RestNotificationEventConsumer : BaseEventConsumer
    {
        #region Consts
        
        private const string CB_SECTION_NAME = "groups";
        private const string CB_SPECIFIC_PARTNER_KEY_FORMAT = "notification_{0}_{1}";
        private const string CB_GENERAL_KEY_FORMAT = "notification_0_{0}";

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

            if (kalturaEvent is KalturaObjectEvent)
            {
                result = true;
            }

            return result;
        }

        protected override eEventConsumptionResult Consume(KalturaEvent kalturaEvent)
        {
            eEventConsumptionResult result = eEventConsumptionResult.None;

            KalturaObjectEvent objectEvent = kalturaEvent as KalturaObjectEvent;
            EventNotification specificNotification = null;
            EventNotification generalNotification = null;
            Dictionary<string, NotificationAction> actions = new Dictionary<string, NotificationAction>();

            string actionString = "None";

            var actionEvent = objectEvent as KalturaObjectActionEvent;

            if (actionEvent != null)
            {
                actionString = actionEvent.Action.ToString();
            }

            log.DebugFormat("Start consume Notification object action event for: partnerId = {0}, type = {1}, action = {2}",
                objectEvent.PartnerId, objectEvent.Type, actionString);
            
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

                string cbKey = GetCBSpecificKey(objectEvent);
                specificNotification = cbManager.Get<EventNotification>(cbKey, true);
            }

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE)
            {
                Database = CB_SECTION_NAME,
                QueryType = KLogEnums.eDBQueryType.SELECT
            })
            {
                string cbKey = GetCBGeneralKey(objectEvent);
                generalNotification = cbManager.Get<EventNotification>(cbKey, true);
            }

            // check if we have the metadata at all. if we don't we don't continue
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

            // Check action statuses
            bool atLeastOneActive = false;

            foreach (var action in actions.Values)
            {
                if (action.Status == 1)
                {
                    atLeastOneActive = true;
                    break;
                }
            }

            if (!atLeastOneActive)
            {
                shouldConsume = false;
            }

            if (!shouldConsume)
            {
                return eEventConsumptionResult.None;
            }

            #endregion

            result = eEventConsumptionResult.Success;

            #region Convert object

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

            var eventWrapper = EventConverter.ConvertEvent(phoenixType, objectEvent);

            #endregion

            #region Handle actions

            // Perform the actions for this event
            foreach (var action in actions.Values)
            {
                log.DebugFormat("Notification event action: action name = {0}, partner {1}, event type {2}, event action {3}, specific notification is {4}", 
                    action.SystemName, kalturaEvent.PartnerId, objectEvent.Type, actionEvent, action.GetType().ToString());

                try
                {
                    PerformAction(action, kalturaEvent, eventWrapper);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat(
                        "Error when performing action event for partner {0}, event type {1}, event action {2}, specific notification is {3}. ex = {4}",
                        kalturaEvent.PartnerId, objectEvent.Type, actionEvent, action.GetType().ToString(), ex);

                    result = eEventConsumptionResult.Failure;
                    if (action.FailureHandlers != null && action.FailureHandlers.Count > 0)
                    {
                        foreach (var handler in action.FailureHandlers)
                        {
                            PerformAction(action, kalturaEvent, eventWrapper);
                        }
                    }
                }
            }

            #endregion

            return result;
        }

        #endregion

        #region Protected methods

        protected void PerformAction(NotificationAction action, KalturaEvent kalturaEvent, KalturaNotification eventWrapper)
        {
            // first check action's status - if it isn't good,
            if (action.Status != 1)
            {
                return;
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
                        conditionResult &= condition.Evaluate(kalturaEvent, eventWrapper.eventObject);
                    }
                }
            }

            if (!conditionResult)
            {
                return;
            }

            if (!action.IsAsync)
            {
                action.Handle(kalturaEvent, eventWrapper);
            }
            else
            {
                // save context data - for multi threading operations
                ContextData contextData = new ContextData();

                HostingEnvironment.QueueBackgroundWorkItem((obj) =>
                {
                    try
                    {
                        contextData.Load();

                        log.DebugFormat("Start async action: action name = {0}, partner {1},  specific notification is {2}",
                            action.SystemName, kalturaEvent.PartnerId, action.GetType().ToString());

                        action.Handle(kalturaEvent, eventWrapper);

                        log.DebugFormat("Finished async action: action name = {0}, partner {1},  specific notification is {2}",
                            action.SystemName, kalturaEvent.PartnerId, action.GetType().ToString());

                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error when performing async action. partner {0}, system name {1}, ex = {2}",
                            kalturaEvent.PartnerId, action.SystemName, ex);
                    }
                });
            }
        }

        protected string GetCBGeneralKey(KalturaObjectEvent kalturaEvent)
        {
            return string.Format(CB_GENERAL_KEY_FORMAT, kalturaEvent.GetSystemName()).ToLower();
        }

        protected string GetCBSpecificKey(KalturaObjectEvent kalturaEvent)
        {
            return string.Format(CB_SPECIFIC_PARTNER_KEY_FORMAT, kalturaEvent.PartnerId, kalturaEvent.GetSystemName()).ToLower();
        }

        #endregion

    }
}