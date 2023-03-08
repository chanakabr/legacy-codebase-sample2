using ApiObjects;
using System;
using System.Web;
using KalturaRequestContext;
using TVinciShared;
using WebAPI.Models.General;

namespace WebAPI.EventNotifications
{
    public class EventConverter
    {
        public static KalturaNotification ConvertEvent(string phoenixType, KalturaObjectEvent objectEvent)
        {
            // Convert the WS object to an API/rest/phoenixObject
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
            long? userId;
            string userIp;
            string udid;

            var contextData = Managers.Models.KS.GetContextData(true);
            if (contextData != null)
            {
                userId = contextData.GetCallerUserId();
                userIp = contextData.UserIp;
                udid = !string.IsNullOrEmpty(contextData.Udid) ? contextData.Udid : null;
            }
            else
            {
                //try get from context
                userIp = RequestContextUtilsInstance.Get().GetUserIp();
                userId = RequestContextUtilsInstance.Get().GetUserId();
                udid = RequestContextUtilsInstance.Get().GetUdid();
            }

            KalturaNotification eventWrapper = new KalturaNotification()
            {
                eventObject = ottObject,
                eventType = action,
                eventObjectType = destination.Name,
                systemName = systemName,
                partnerId = objectEvent.PartnerId,
                UserIp = userIp,
                SequenceId = RequestContextUtilsInstance.Get().GetRequestId(),
                UserId = userId,
                Context = GetContext(),
                Udid = udid,
                CreateDate = DateUtils.GetUtcUnixTimestampNow()
            };

            return eventWrapper;
        }

        private static KalturaEventContextAction GetContext()
        {
            if (HttpContext.Current?.Items != null)
            {
                var context = new KalturaEventContextAction();
                if (HttpContext.Current.Items.ContainsKey(RequestContextConstants.REQUEST_SERVICE))
                {
                    context.Service = Convert.ToString(HttpContext.Current.Items[RequestContextConstants.REQUEST_SERVICE]);
                }

                if (HttpContext.Current.Items.ContainsKey(RequestContextConstants.REQUEST_ACTION))
                {
                    context.Action = Convert.ToString(HttpContext.Current.Items[RequestContextConstants.REQUEST_ACTION]);
                }

                if (!string.IsNullOrEmpty(context.Action) && !string.IsNullOrEmpty(context.Service))
                {
                    return context;
                }
            }
            return null;
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