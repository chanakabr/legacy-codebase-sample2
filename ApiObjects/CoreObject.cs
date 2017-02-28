using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    [Serializable]
    public abstract class CoreObject
    {
        public int GroupId
        {
            get;
            set;
        }

        public long Id
        {
            get;
            set;
        }

        public List<string> ChangedFields
        {
            get;
            set;
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
                EventManager.EventManager.HandleEvent(new KalturaObjectActionEvent(
                    this.GroupId,
                    this,
                    eKalturaEventActions.Created,
                    eKalturaEventTime.Before));

            bool shouldInsert = true;

            // Check results - if one of the consumers failed, don't insert
            if (beforeInsertEventResults != null)
            {
                foreach (var eventResult in beforeInsertEventResults)
                {
                    if (eventResult == EventManager.eEventConsumptionResult.Failure)
                    {
                        shouldInsert = false;
                        break;
                    }
                }
            }

            bool insertResult = false;

            // Do the insert if we agreed on doing it, and raise new event
            if (shouldInsert)
            {
                 insertResult = DoInsert();

                 if (insertResult)
                 {
                     var afterEventResults = EventManager.EventManager.HandleEvent(new KalturaObjectActionEvent(
                         this.GroupId,
                         this,
                         eKalturaEventActions.Created));

                     if (afterEventResults != null)
                     {
                         // ?
                     }
                 }
                 else
                 {
                     EventManager.EventManager.HandleEvent(new KalturaObjectActionEvent(
                         this.GroupId,
                         this,
                         eKalturaEventActions.Created, eKalturaEventTime.Failed));
                 }
            }

            return insertResult;
        }

        public bool Update()
        {
            // Raise event before starting to update
            var beforeEventResults =
                EventManager.EventManager.HandleEvent(new KalturaObjectActionEvent(
                    this.GroupId,
                    this,
                    eKalturaEventActions.Changed,
                    eKalturaEventTime.Before));

            bool shouldUpdate = true;

            // Check results - if one of the consumers failed, don't update
            if (beforeEventResults != null)
            {
                foreach (var eventResult in beforeEventResults)
                {
                    if (eventResult == EventManager.eEventConsumptionResult.Failure)
                    {
                        shouldUpdate = false;
                        break;
                    }
                }
            }

            bool result = false;

            // Do the update if we agreed on doing it, and raise new event
            if (shouldUpdate)
            {
                CoreObject previous = this.CoreClone();

                result = DoUpdate();

                if (result)
                {
                    EventManager.EventManager.HandleEvent(new KalturaObjectChangedEvent(
                        this.GroupId,
                        this,
                        previous,
                        this.ChangedFields));
                }
                else
                {
                    EventManager.EventManager.HandleEvent(new KalturaObjectChangedEvent(
                        this.GroupId,
                        this,
                        previous,
                        this.ChangedFields,
                        eKalturaEventTime.Failed));
                }
            }

            return result;
        }

        public bool Delete()
        {
            // Raise event before starting to delete
            var beforeEventResults =
                EventManager.EventManager.HandleEvent(new KalturaObjectActionEvent(
                    this.GroupId,
                    this,
                    eKalturaEventActions.Deleted,
                    eKalturaEventTime.Before));

            bool shouldDelete = true;

            // Check results - if one of the consumers failed, don't delete
            if (beforeEventResults != null)
            {
                foreach (var eventResult in beforeEventResults)
                {
                    if (eventResult == EventManager.eEventConsumptionResult.Failure)
                    {
                        shouldDelete = false;
                        break;
                    }
                }
            }

            bool result = false;

            if (shouldDelete)
            {
                result = DoDelete();

                if (result)
                {
                    EventManager.EventManager.HandleEvent(new KalturaObjectDeletedEvent(
                        this.GroupId,
                        this.Id,
                        null,
                        this));
                }
                else
                {
                    EventManager.EventManager.HandleEvent(new KalturaObjectDeletedEvent(
                        this.GroupId,
                        this.Id,
                        null,
                        this,
                        eKalturaEventTime.Failed));
                }
            }

            return result;
        }

        #endregion

    }
}
