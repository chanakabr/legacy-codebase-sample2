using Phx.Lib.Log;
using Newtonsoft.Json;
using System.Reflection;

namespace DAL
{
    public class Json
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static bool TryDeserialize<T>(string json, out T deserializedObject)
        {
            deserializedObject = default(T);
            var success = true;
            var jsonSerializerSettings =
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    Error = (sender, args) =>
                    {
                        if (args.ErrorContext.Error is JsonSerializationException &&
                            args.ErrorContext.Path.Contains("$type") &&
                            args.ErrorContext.OriginalObject != null)
                        {
                            args.ErrorContext.Handled = true;
                        }
                        log.Warn($"TryDeserialize failed, error: {args.ErrorContext.Error}");
                        success = false;
                    }
                };

            var deserialized = JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
            if (success)
            {
                deserializedObject = deserialized;
            }
            return success;
        }
    }
}
