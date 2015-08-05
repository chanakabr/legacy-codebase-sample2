using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.MediaIndexingObjects
{
    [Serializable]
    public class BaseCeleryData : QueueObject
    {
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
                this.eta = value.ToString("o");
            }
        }

        public DateTime Expires
        {
            set
            {
                this.expires = value.ToString("o");
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
                this.eta = eta.Value.ToString("o");
            }

            if (expires != null && expires.HasValue)
            {
                this.expires = expires.Value.ToString("o");
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
