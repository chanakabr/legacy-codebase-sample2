using ApiObjects.MediaIndexingObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    public class BaseCeleryData : QueueObject
    {
        public const string CELERY_DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss.ffffffZ";

        public string id;
        public string task;
        public List<object> args;
        public object kwargs;
        public string eta;
        public string expires;

        // recovery ID is by default the message ID. (could be overridden in descendants)
        private string _recoveryMessageId = string.Empty;

        [JsonIgnore]
        public string RecoveryMessageId
        {
            get
            {
                if (string.IsNullOrEmpty(this._recoveryMessageId))
                    return this.id;
                else
                    return this._recoveryMessageId;
            }
            set
            {
                this._recoveryMessageId = value;
            }
        }

        [JsonIgnore]
        public DateTime? ETA
        {
            get
            {
                if (!string.IsNullOrEmpty(eta))
                    return DateTime.ParseExact(eta, CELERY_DATE_FORMAT, System.Globalization.CultureInfo.CurrentCulture);
                else
                    return null;
            }
            set
            {
                this.eta = value.Value.ToString(CELERY_DATE_FORMAT);
            }
        }

        [JsonIgnore]
        public DateTime? Expires
        {
            get
            {
                if (!string.IsNullOrEmpty(expires))
                    return DateTime.ParseExact(expires, CELERY_DATE_FORMAT, System.Globalization.CultureInfo.CurrentCulture);
                else
                    return null;
            }
            set
            {
                this.expires = value.Value.ToString(CELERY_DATE_FORMAT);
            }
        }

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
