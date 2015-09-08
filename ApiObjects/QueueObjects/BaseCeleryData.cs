using ApiObjects.MediaIndexingObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    public class BaseCeleryData : QueueObject
    {
        #region Consts

        public const string CELERY_DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss.ffffffZ";
        #endregion
        #region Properties

        public string id;
        public string task;
        public List<object> args;
        public object kwargs;
        public string eta;
        public string expires;

        public DateTime ETA
        {
            set
            {
                this.eta = value.ToString(CELERY_DATE_FORMAT);
            }
        }

        public DateTime Expires
        {
            set
            {
                this.expires = value.ToString(CELERY_DATE_FORMAT);
            }
        }

        #endregion

        public BaseCeleryData()
        {
            kwargs = new object();
            args = new List<object>();
        }

        public BaseCeleryData(string id, string task, List<object> args, DateTime? eta = null, DateTime? expires = null)
        {
            kwargs = new object();
            this.task = task;
            this.id = id;       
            this.args = args;

            if (eta != null && eta.HasValue)
            {
                this.eta = eta.Value.ToString(CELERY_DATE_FORMAT);
            }

            if (expires != null && expires.HasValue)
            {
                this.expires = expires.Value.ToString(CELERY_DATE_FORMAT);
            }
        }

        public BaseCeleryData(string id, string task, params object[] args)
        {
            kwargs = new object();
            this.task = task;
            this.id = id;
            this.args = new List<object>();
            this.args.AddRange(args);
        }
    }
}
