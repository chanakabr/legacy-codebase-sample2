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
using WebAPI.Managers.Scheme;
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

            Version currentVersion = OldStandardAttribute.getCurrentRequestVersion();
            if (value is IKalturaSerializable)
            {
                result = ((IKalturaSerializable)value).ToJson(currentVersion, omitObsolete);
            }
            else
            {
                result = JsonConvert.SerializeObject(value);
            }

            return result;
        }
    }
}