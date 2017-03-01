using ApiObjects;
using EventManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.EventNotifications
{
    public class EventConverter
    {
        public static KalturaNotification ConvertEvent(string phoenixType, KalturaObjectActionEvent actionEvent)
        {
            // convert the WS object to an API/rest/phoenixObject
            object phoenixObject = null;
            KalturaOTTObject ottObject = null;

            // Get the phoenix type Type
            Type destination = Type.GetType(phoenixType);

            // If we have an object, convert it normally
            if (actionEvent.Object != null)
            {
                Type sourceType = actionEvent.Object.GetType();

                phoenixObject = AutoMapper.Mapper.Map(actionEvent.Object, sourceType, destination);
                ottObject = phoenixObject as KalturaOTTObject;
            }
            else
            {
                // Otherwise we need to start
                var deleteEvent = actionEvent as KalturaObjectDeletedEvent;

                if (deleteEvent != null)
                {
                    ottObject = new KalturaLongValue()
                    {
                        description = "id",
                        value = deleteEvent.Id
                    };
                }
            }

            KalturaNotification eventWrapper = new KalturaNotification()
            {
                eventObject = ottObject,
                eventType = ConvertKalturaAction(actionEvent.Action),
                objectType = destination.Name
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