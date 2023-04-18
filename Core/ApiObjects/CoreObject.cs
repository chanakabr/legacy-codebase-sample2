using EventManager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiObjects
{
    [Serializable]
    public abstract class CoreObject
    {
        [JsonProperty("GroupId")] public int GroupId { get; set; }

        [JsonProperty("Id")] public long Id { get; set; }

        public List<string> ChangedFields { get; set; }

        protected CoreObject()
        {
        }

        protected CoreObject(CoreObject other)
        {
            GroupId = other.GroupId;
            Id = other.Id;
            ChangedFields = other.ChangedFields;
        }

        #region Abstract Methods

        protected abstract bool DoInsert();
        protected abstract bool DoUpdate();
        protected abstract bool DoDelete();
        public abstract CoreObject CoreClone();

        #endregion

        #region Public Methods

        public bool Insert()
        {
            // Raise event before starting to insert
            var beforeInsertEventResults =
                EventManager.EventManager.HandleEvent(new KalturaObjectActionEvent(GroupId, this,
                    eKalturaEventActions.Created, eKalturaEventTime.Before));

            // Check results - if one of the consumers failed, don't insert
            bool shouldInsert =
                beforeInsertEventResults != null &&
                beforeInsertEventResults.Any(x => x == eEventConsumptionResult.Failure)
                    ? false
                    : true;
            bool insertResult = false;

            // Do the insert if we agreed on doing it, and raise new event
            if (shouldInsert)
            {
                insertResult = DoInsert();

                if (insertResult)
                {
                    var afterEventResults =
                        EventManager.EventManager.HandleEvent(
                            new KalturaObjectActionEvent(GroupId, this, eKalturaEventActions.Created));
                    if (afterEventResults != null)
                    {
                        // ?
                    }
                }
                else
                {
                    EventManager.EventManager.HandleEvent(new KalturaObjectActionEvent(GroupId, this,
                        eKalturaEventActions.Created, eKalturaEventTime.Failed));
                }
            }

            return insertResult;
        }

        public bool Update()
        {
            // Raise event before starting to update
            var beforeEventResults =
                EventManager.EventManager.HandleEvent(new KalturaObjectActionEvent(GroupId, this,
                    eKalturaEventActions.Changed, eKalturaEventTime.Before));

            // Check results - if one of the consumers failed, don't update
            bool shouldUpdate =
                beforeEventResults != null && beforeEventResults.Any(x => x == eEventConsumptionResult.Failure)
                    ? false
                    : true;
            bool result = false;

            // Do the update if we agreed on doing it, and raise new event
            if (shouldUpdate)
            {
                CoreObject previous = CoreClone();

                result = DoUpdate();

                if (result)
                {
                    EventManager.EventManager.HandleEvent(new KalturaObjectChangedEvent(GroupId, this, previous,
                        ChangedFields));
                }
                else
                {
                    EventManager.EventManager.HandleEvent(new KalturaObjectChangedEvent(GroupId, this, previous,
                        ChangedFields, eKalturaEventTime.Failed));
                }
            }

            return result;
        }

        public bool Delete()
        {
            // Raise event before starting to delete
            var beforeEventResults =
                EventManager.EventManager.HandleEvent(new KalturaObjectActionEvent(GroupId, this,
                    eKalturaEventActions.Deleted, eKalturaEventTime.Before));

            // Check results - if one of the consumers failed, don't delete
            bool shouldDelete =
                beforeEventResults != null && beforeEventResults.Any(x => x == eEventConsumptionResult.Failure)
                    ? false
                    : true;
            bool result = false;

            if (shouldDelete)
            {
                result = DoDelete();

                if (result)
                {
                    EventManager.EventManager.HandleEvent(new KalturaObjectDeletedEvent(GroupId, Id, null, this));
                }
                else
                {
                    EventManager.EventManager.HandleEvent(new KalturaObjectDeletedEvent(GroupId, Id, null, this,
                        eKalturaEventTime.Failed));
                }
            }

            return result;
        }

        public bool Notify(eKalturaEventTime? time = eKalturaEventTime.After, string type = null)
        {
            bool result = true;

            List<eEventConsumptionResult> afterEventResults = null;

            // If we have time, it is an action event, otherwise it is just an object event.
            if (time != null && time.HasValue)
            {
                afterEventResults = EventManager.EventManager.HandleEvent(
                    new KalturaObjectActionEvent(GroupId, this, eKalturaEventActions.None, time.Value, type));
            }
            else
            {
                afterEventResults = EventManager.EventManager.HandleEvent(new KalturaObjectEvent(GroupId, this, type));
            }

            // check that we actually have results before running on list...
            if (afterEventResults == null)
            {
                result = false;
            }
            else
            {
                result = afterEventResults.Any(x => x == eEventConsumptionResult.Failure) ? false : result;
            }

            return result;
        }

        #endregion
    }
}