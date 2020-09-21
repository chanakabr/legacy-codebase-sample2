using Google.Protobuf.Reflection;
using KLogMonitor;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RedisManager;
using SoapAdaptersCommon.Helpers;
using SoapCore;
using SoapCore.Extensibility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoapAdaptersCommon.Middleware
{
    public class AdapterOperationInvoker : IOperationInvoker
    {
        private const string RESPONSE_RECORDING_NAMESPACE_KEY = "RESPONSE_RECORDING_NAMESPACE";
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly DefaultOperationInvoker _DefaultSoapCoreInvoker;
        private readonly IAdapterRequestContextAccessor _AdapterRequestContextAccessor;

        public AdapterOperationInvoker(IAdapterRequestContextAccessor adapterRequestContextAccessor)
        {
            _Logger.Info($"Initilizing AdapterOperationInvoker...");
            _DefaultSoapCoreInvoker = new DefaultOperationInvoker();
            _AdapterRequestContextAccessor = adapterRequestContextAccessor;
        }


        public async Task<object> InvokeAsync(MethodInfo methodInfo, object instance, object[] inputs)
        {
            var keyNamespace = Environment.GetEnvironmentVariable(RESPONSE_RECORDING_NAMESPACE_KEY);
            if (string.IsNullOrEmpty(keyNamespace))
            {
                keyNamespace = Assembly.GetEntryAssembly().GetName().Name;
            }

            var key = _AdapterRequestContextAccessor.Current.RequestId;
            key = $"{keyNamespace}:{key}";
            _Logger.Debug($"calculated key:[{key}]");
            var result = await _DefaultSoapCoreInvoker.InvokeAsync(methodInfo, instance, inputs);

            _ = Task.Run(() =>
            {
                var isSetSuccess = RedisClientManager.Instance.Set(key, result, TimeSpan.FromMinutes(5).TotalSeconds, new JsonSerializerSettings());
                if (!isSetSuccess)
                {
                    _Logger.Error($"Failed to recorder response for key:[{key}] into redis");
                }
            });

            return result;
        }







    }
}