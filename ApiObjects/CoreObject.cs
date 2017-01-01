using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public abstract class CoreObject
    {
        protected int GroupId
        {
            get;
            set;
        }

        protected long Id
        {
            get;
            set;
        }

        #region Abstract Methods

        public abstract bool DoInsert();
        public abstract bool DoUpdate();
        public abstract bool DoDelete();
        public abstract CoreObject CoreClone();

        #endregion

        #region Public Methods

        public bool Insert()
        {
            bool result = DoInsert();

            if (result)
            {
                EventManager.EventManager.HandleEvent(new KalturaObjectActionEvent(
                    this.GroupId,
                    this,
                    eKalturaEventActions.Created));
            }

            return result;
        }

        public bool Update()
        {
            CoreObject previous = this.CoreClone();

            bool result = DoUpdate();

            if (result)
            {
                EventManager.EventManager.HandleEvent(new KalturaObjectChangedEvent(
                    this.GroupId,
                    this,
                    previous));
            }

            return result;
        }

        public bool Delete()
        {
            bool result = DoDelete();

            if (result)
            {
                EventManager.EventManager.HandleEvent(new KalturaObjectDeletedEvent(
                    this.GroupId,
                    this.Id));
            }

            return result;
        }

        #endregion

    }
}
