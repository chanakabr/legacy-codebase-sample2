using ApiObjects;
using KLogMonitor;
using System;
using System.Web;
using TVinciShared;
using WebAPI.Models.General;

namespace WebAPI.EventNotifications
{
    public class EventConverter
    {
        public static KalturaNotification ConvertEvent(string phoenixType, KalturaObjectEvent objectEvent)
        {
            // convert the WS object to an API/rest/phoenixObject
            object phoenixObject = null;
            KalturaOTTObject ottObject = null;

            // Get the phoenix type Type
            Type destination = Type.GetType(phoenixType);

            // If we have an object, convert it normally
            if (objectEvent.Object != null)
            {
                Type sourceType = objectEvent.Object.GetType();

                phoenixObject = AutoMapper.Mapper.Map(objectEvent.Object, sourceType, destination);
                ottObject = phoenixObject as KalturaOTTObject;
            }
            else
            {
                // Otherwise we need to start
                var deleteEvent = objectEvent as KalturaObjectDeletedEvent;

                if (deleteEvent != null)
                {
                    ottObject = new KalturaLongValue()
                    {
                        description = "id",
                        value = deleteEvent.Id
                    };
                }
            }

            KalturaEventAction action = KalturaEventAction.None;

            KalturaObjectActionEvent actionEvent = objectEvent as KalturaObjectActionEvent;
            if (actionEvent != null)
            {
                action = ConvertKalturaAction(actionEvent.Action);
            }

            string systemName = objectEvent.GetSystemName();
            long? userId = null;
            string userIp = null;
            string udid = null;

            var contextData = WebAPI.Managers.Models.KS.GetContextData(true);
            if (contextData != null)
            {
                userId = contextData.OriginalUserId > 0 ? contextData.OriginalUserId : contextData.UserId;
                userIp = contextData.UserIp;
                udid = !string.IsNullOrEmpty(contextData.Udid) ? contextData.Udid : null;
            }
            else
            {
                //try get from context
                userIp = HttpContext.Current?.Items[RequestContextUtils.USER_IP]?.ToString();

                if (HttpContext.Current.Items.ContainsKey(RequestContextUtils.REQUEST_USER_ID))
                {
                    userId = long.Parse(HttpContext.Current.Items[RequestContextUtils.REQUEST_USER_ID].ToString());
                }

                udid = HttpContext.Current?.Items[RequestContextUtils.REQUEST_UDID]?.ToString();
            }

            KalturaNotification eventWrapper = new KalturaNotification()
            {
                eventObject = ottObject,
                eventType = action,
                eventObjectType = destination.Name,
                systemName = systemName,
                partnerId = objectEvent.PartnerId,
                UserIp = userIp,
                SequenceId = HttpContext.Current?.Items[Constants.REQUEST_ID_KEY]?.ToString(),
                UserId = userId,
                Udid = udid
            };

            return eventWrapper;
        }

        internal static KalturaEventAction ConvertKalturaAction(eKalturaEventActions eKalturaEventActions)
        {
            KalturaEventAction action = KalturaEventAction.None;
            switch (eKalturaEventActions)
            {
                case eKalturaEventActions.None:
                {
                    action = KalturaEventAction.None;
                    break;
                }
                case eKalturaEventActions.Added:
                {
                    action = KalturaEventAction.Added;
                    break;
                }
                case eKalturaEventActions.Changed:
                {
                    action = KalturaEventAction.Changed;
                    break;
                }
                case eKalturaEventActions.Copied:
                {
                    action = KalturaEventAction.Copied;
                    break;
                }
                case eKalturaEventActions.Created:
                {
                    action = KalturaEventAction.Created;
                    break;
                }
                case eKalturaEventActions.Deleted:
                {
                    action = KalturaEventAction.Deleted;
                    break;
                }
                case eKalturaEventActions.Erased:
                {
                    action = KalturaEventAction.Erased;
                    break;
                }
                case eKalturaEventActions.Saved:
                {
                    action = KalturaEventAction.Saved;
                    break;
                }
                case eKalturaEventActions.Updated:
                {
                    action = KalturaEventAction.Updated;
                    break;
                }
                case eKalturaEventActions.Replaced:
                {
                    action = KalturaEventAction.Replaced;
                    break;
                }
                default:
                break;
            }

            return action;
        }
    }
}