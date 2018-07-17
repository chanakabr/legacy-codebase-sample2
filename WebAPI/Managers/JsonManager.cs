using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using WebAPI.Filters;
using WebAPI.Models.General;
using WebAPI.Reflection;

namespace WebAPI.Managers
{
    public class JsonManager
    {
        private static JsonManager instance;

        private JsonManager()
        {
        }

        public static JsonManager GetInstance()
        {
            if (instance == null)
            {
                instance = new JsonManager();
            }

            return instance;
        }
                        
        public string Serialize(object value, bool omitObsolete = false)
        {
            string result = string.Empty;

            if (value is IKalturaSerializable)
            {
                object requestVersion = null;
                Version currentVersion = null;

                if (HttpContext.Current != null && HttpContext.Current.Items != null)
                {
                    requestVersion = HttpContext.Current.Items[RequestParser.REQUEST_VERSION];
                }

                if (requestVersion != null)
                {
                    currentVersion = HttpContext.Current.Items[RequestParser.REQUEST_VERSION] as Version;
                }

                try
                {
                    result = ((IKalturaSerializable)value).ToJson(currentVersion, omitObsolete);
                }
                catch
                {
                    result = null;
                }
            }

            if (string.IsNullOrEmpty(result))
            {
                result = JsonConvert.SerializeObject(value);
            }

            return result;
        }
    }
}