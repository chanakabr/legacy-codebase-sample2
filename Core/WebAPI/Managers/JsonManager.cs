using Newtonsoft.Json;
using System;
using System.Text;
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

        [Obsolete]
        public string ObsoleteSerialize(object value, bool omitObsolete = false)
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

        public StringBuilder Serialize(object value, bool omitObsolete = false)
        {
            var stringBuilder = Serialize(new StringBuilder(), value, omitObsolete);

            return stringBuilder;
        }

        public StringBuilder Serialize(StringBuilder stringBuilder, object value, bool omitObsolete = false)
        {
            if (value is IKalturaSerializable kalturaSerializable)
            {
                var currentVersion = OldStandardAttribute.getCurrentRequestVersion();
                kalturaSerializable.AppendAsJson(stringBuilder, currentVersion, omitObsolete);
            }
            else
            {
                var stringJson = JsonConvert.SerializeObject(value);
                stringBuilder.Append(stringJson);
            }

            return stringBuilder;
        }
    }
}