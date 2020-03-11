using Newtonsoft.Json;
using System;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

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