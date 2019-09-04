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

        public GenericResponse<EventNotificationAction> Add(ContextData contextData, EventNotificationAction eventNotificationActionToAdd)
        {
            var response = new GenericResponse<EventNotificationAction>();

            try
            {
                eventNotificationActionToAdd.CreateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
                eventNotificationActionToAdd.UpdateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

                // Save eventNotificationAction by id At CB                    
                if (!ApiDAL.SaveEventNotificationActionIdCB(eventNotificationActionToAdd))
                {
                    log.Error($"Error while SaveEventNotificationActionCB. contextData: {contextData.ToString()}");
                    return response;
                }

                // EventNotificationAction by object Type & Id
                List<string> eventNotificationActions = ApiDAL.GetEventNotificationActionCB(eventNotificationActionToAdd.ObjectType, eventNotificationActionToAdd.ObjectId);
                if (eventNotificationActions?.Count > 0)
                {
                    eventNotificationActions.Add(eventNotificationActionToAdd.Id);
                }
                else
                {
                    eventNotificationActions = new List<string>() { eventNotificationActionToAdd.Id };
                }

                // Save eventNotificationAction by type and id At CB                    
                if (!ApiDAL.SaveEventNotificationActionTypeAndIdCB(eventNotificationActionToAdd.ObjectType, eventNotificationActionToAdd.ObjectId, eventNotificationActions))
                {
                    log.Error($"Error while SaveEventNotificationActionCB. contextData: {contextData.ToString()}");
                    return response;
                }

                response.Object = eventNotificationActionToAdd;
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in SaveEventNotificationActionCB. contextData:{contextData.ToString()}. ex: {ex}");
            }

            return response;
        }

        public GenericResponse<EventNotificationAction> Update(ContextData contextData, EventNotificationAction eventNotificationActionToUpdate)
        {
            var response = new GenericResponse<EventNotificationAction>();

            try
            {
                // get current EventNotificationAction 
                EventNotificationAction currentEventNotificationAction = ApiDAL.GetEventNotificationActionCB(eventNotificationActionToUpdate.Id);
                if (currentEventNotificationAction == null)
                {
                    log.Error($"EventNotificationAction wasn't found id {eventNotificationActionToUpdate.Id}");
                    response.SetStatus(eResponseStatus.EventNotificationIdNotFound);
                    return response;
                }

                eventNotificationActionToUpdate.UpdateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
                eventNotificationActionToUpdate.Status = eventNotificationActionToUpdate.Status;
                eventNotificationActionToUpdate.Message = eventNotificationActionToUpdate.Message;
               
                if (!ApiDAL.SaveEventNotificationActionIdCB(eventNotificationActionToUpdate))
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

        public GenericResponse<EventNotificationAction> Get(ContextData contextData, string id)
        {
            throw new NotImplementedException();
        }

        public GenericListResponse<EventNotificationAction> List(ContextData contextData, EventNotificationActionFilter filter)
        {
            var response = new GenericListResponse<EventNotificationAction>();

            List<EventNotificationAction> eventNotificationActions = null;
            EventNotificationAction eventNotificationAction = null;

            if (filter != null && filter.ObjectId.HasValue && filter.ObjectId.Value > 0)
            {
                var eventNotificationActionIds = ApiDAL.GetEventNotificationActionCB(filter.ObjectType, filter.ObjectId.Value);
                if (eventNotificationActionIds?.Count > 0)
                {
                    eventNotificationActions = new List<EventNotificationAction>();
                    foreach (var id in eventNotificationActionIds)
                    {
                        eventNotificationAction = ApiDAL.GetEventNotificationActionCB(id);
                        if (eventNotificationAction != null)
                        {
                            eventNotificationActions.Add(eventNotificationAction);
                        }
                    }

                    response.Objects = eventNotificationActions;
                    response.TotalItems = eventNotificationActions.Count;
                    response.SetStatus(eResponseStatus.OK);
                }
            }
            else if (filter != null && !string.IsNullOrEmpty(filter.Id))
            {
                eventNotificationAction = ApiDAL.GetEventNotificationActionCB(filter.Id);
                eventNotificationActions = new List<EventNotificationAction>() { eventNotificationAction };

                response.Objects = eventNotificationActions;
                response.TotalItems = 1;
                response.SetStatus(eResponseStatus.OK);
            }

            return response;
        }
    }
}