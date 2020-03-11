using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using TVinciShared;

namespace ApiLogic.Api.Managers
{
    public class EventNotificationActionManager : ICrudHandler<EventNotificationAction, string, EventNotificationActionFilter>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<EventNotificationActionManager> lazy = new Lazy<EventNotificationActionManager>(() => new EventNotificationActionManager());

        public static EventNotificationActionManager Instance { get { return lazy.Value; } }

        private EventNotificationActionManager() { }

        public void SaveEventNotificationAction(int groupId, EventNotificationAction eventNotificationAction)
        {
            long epocNow = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

            eventNotificationAction.CreateDate = epocNow;
            eventNotificationAction.UpdateDate = epocNow;

            // Save eventNotificationAction by id At CB                    
            if (!ApiDAL.SaveEventNotificationActionIdCB(groupId, eventNotificationAction))
            {
                log.Error($"Error while SaveEventNotificationActionCB. group id: { groupId }");
            }
        }

        public void SaveEventNotificationObjectActions(int groupId, List<string> ids, string objectType, long objectId)
        {
            try
            {
                // Save eventNotificationAction by type and id At CB                    
                if (!ApiDAL.SaveEventNotificationActionTypeAndIdCB(groupId, objectType, objectId, ids))
                {
                    log.Error($"Error while SaveEventNotificationActionCB");
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in SaveEventNotificationActionCB. ex: {ex}");
            }
        }

        public GenericResponse<EventNotificationAction> Update(ContextData contextData, EventNotificationAction eventNotificationActionToUpdate)
        {
            var response = new GenericResponse<EventNotificationAction>();

            try
            {
                // get current EventNotificationAction 
                EventNotificationAction currentEventNotificationAction = ApiDAL.GetEventNotificationActionCB(contextData.GroupId, eventNotificationActionToUpdate.Id);
                if (currentEventNotificationAction == null)
                {
                    log.Error($"EventNotificationAction wasn't found id {eventNotificationActionToUpdate.Id}");
                    response.SetStatus(eResponseStatus.EventNotificationIdNotFound);
                    return response;
                }

                eventNotificationActionToUpdate.UpdateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
                eventNotificationActionToUpdate.Status = eventNotificationActionToUpdate.Status;
                eventNotificationActionToUpdate.Message = eventNotificationActionToUpdate.Message;
               
                if (!ApiDAL.SaveEventNotificationActionIdCB(contextData.GroupId, eventNotificationActionToUpdate))
                {
                    log.Error($"Error while saving EventNotificationAction id {eventNotificationActionToUpdate.Id}");
                }
                else
                {
                    response.Object = eventNotificationActionToUpdate;
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error($"Update EventNotificationAction failed ex={ex},  eventNotificationActionId={ eventNotificationActionToUpdate.Id}");
            }

            return response;
        }

        public Status Delete(ContextData contextData, string id)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<EventNotificationAction> Add(ContextData contextData, EventNotificationAction eventNotificationActionToAdd)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<EventNotificationAction> Get(ContextData contextData, string id)
        {
            throw new NotImplementedException();
        }

        public GenericListResponse<EventNotificationAction> List(ContextData contextData, EventNotificationActionFilter filter)
        {
            var response = new GenericListResponse<EventNotificationAction>();
            try
            {
                List<EventNotificationAction> eventNotificationActions = null;

                if (filter != null && filter.ObjectId.HasValue && filter.ObjectId.Value > 0)
                {
                    var eventNotificationActionIds = ApiDAL.GetEventNotificationActionCB(contextData.GroupId, filter.ObjectType, filter.ObjectId.Value);
                    if (eventNotificationActionIds?.Count > 0)
                    {
                        eventNotificationActions = ApiDAL.GetEventNotificationActionsCB(contextData.GroupId, eventNotificationActionIds);
                    }
                }
                else if (filter != null && !string.IsNullOrEmpty(filter.Id))
                {
                    EventNotificationAction eventNotificationAction = ApiDAL.GetEventNotificationActionCB(contextData.GroupId, filter.Id);

                    if (eventNotificationAction != null)
                    {
                        eventNotificationActions = new List<EventNotificationAction>() { eventNotificationAction };
                    }
                }

                if (eventNotificationActions?.Count > 0)
                {
                    response.Objects = eventNotificationActions;
                    response.TotalItems = eventNotificationActions.Count;
                    response.SetStatus(eResponseStatus.OK);
                }

            }
            catch (Exception ex)
            {
                log.Error($"Failed to retrive EventNotificationAction list groupID: {contextData.GroupId}, ex: {ex}");
            }

            return response;
        }

        public GenericResponse<EventNotificationAction> ValidateCrudObject(ContextData contextData, string id = null, EventNotificationAction objectToValidate = null)
        {
            throw new NotImplementedException();
        }
    }
}