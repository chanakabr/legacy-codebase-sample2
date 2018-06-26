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
            if(value is IKalturaJsonable)
            {
                Version currentVersion = (Version)HttpContext.Current.Items[RequestParser.REQUEST_VERSION];
                return ((IKalturaJsonable) value).ToJson(currentVersion, omitObsolete);
            }
            return JsonConvert.SerializeObject(value);
        }
    }
}