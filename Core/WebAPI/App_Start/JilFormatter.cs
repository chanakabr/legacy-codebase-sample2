using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using FeatureFlag;
using Phx.Lib.Log;
using WebAPI.Managers;
using WebAPI.Models.API;

namespace WebAPI.App_Start
{
    public class JilFormatter : BaseFormatter
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly JsonManager _jsonManager;
        private readonly IPhoenixFeatureFlag _phoenixFeatureFlag;

        public JilFormatter(KalturaResponseType fortmat = KalturaResponseType.JSON) : base(fortmat, "application/json")
        {
            _jsonManager = JsonManager.GetInstance();
            _phoenixFeatureFlag = PhoenixFeatureFlagInstance.Get();
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return true;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
            using (TextWriter streamWriter = new StreamWriter(writeStream))
            {
                if (_phoenixFeatureFlag.IsEfficientSerializationUsed())
                {
                    var jsonBuilder = _jsonManager.Serialize(value);
#if NETCOREAPP3_1
                    return streamWriter.WriteAsync(jsonBuilder);
#endif
#if NET48
                    return streamWriter.WriteAsync(jsonBuilder.ToString());
#endif
                }
                else
                {
                    var json = _jsonManager.ObsoleteSerialize(value);
                    return streamWriter.WriteAsync(json);
                }
            }
        }

        /// <summary>
        /// This method is used for the .net core version of phoenix and will serialize the object async
        /// </summary>
        public override Task<string> GetStringResponse(object obj)
        {
            return Task.Run(() =>
            {
                string json;
                if (_phoenixFeatureFlag.IsEfficientSerializationUsed())
                {
                    var jsonBuilder = _jsonManager.Serialize(obj);
                    json = jsonBuilder.ToString();
                }
                else
                {
                    json = _jsonManager.ObsoleteSerialize(obj);
                }

                return json;
            });
        }
    }
}