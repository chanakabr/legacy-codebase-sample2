using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;

namespace WebAPI.Utils
{
    public class SerializationUtils
    {
        public class ConvertCommaDelimitedList<T> : CollectionModelBinder<T>
        {
            public override bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                var _queryName = HttpUtility.ParseQueryString(actionContext.Request.RequestUri.Query)[bindingContext.ModelName];
                List<string> _model = new List<string>();
                if (!String.IsNullOrEmpty(_queryName))
                    _model = _queryName.Split(',').ToList();

                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                    bindingContext.Model = _model.ConvertAll(m => (T)converter.ConvertFromString(m));
                else
                    bindingContext.Model = _model;

                return true;
            }
        }

        //TODO: Change. but keep hardcoded, as TCM config may damage this.
        private const string passPhrase = "OdedIsOded";

        public static string MaskSensitiveObject(string originalVal)
        {
            return EncryptionUtils.Encrypt(originalVal, passPhrase);
        }

        public static string UnmaskSensitiveObject(string maskedVal)
        {
            return EncryptionUtils.Decrypt(maskedVal, passPhrase);
        }

        public static DateTime ConvertFromUnixTimestamp(long timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        public static long ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return (long)diff.TotalSeconds;
        }


        public static long? ConvertToUnixTimestamp(DateTime? date)
        {
            if (date != null && date.HasValue)
            {
                return ConvertToUnixTimestamp(date.Value);
            }
            else
            {
                return null;
            }
        }

        public static long GetCurrentUtcTimeInUnixTimestamp()
        {
            return ConvertToUnixTimestamp(DateTime.UtcNow);
        }
    }
}