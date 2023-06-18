using Phx.Lib.Log;
using System.Reflection;
using TVinciShared;
using APILogic.SmsAdapterService;
using ApiObjects;
using System.Linq;
using System;
using Phx.Lib.Appconfig;

namespace ApiLogic.Notification
{
    public class SMSAdapterManager
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static ServiceClient GetSMSAdapterServiceClient(string adapterUrl)
        {
            _Logger.Debug($"Constructing SmsAdapterService Client with url:[{adapterUrl}]");
            var adapaterServiceEndpointConfiguration = ServiceClient.EndpointConfiguration.BasicHttpBinding;
            var adapterClient = new ServiceClient(adapaterServiceEndpointConfiguration, adapterUrl);
            adapterClient.ConfigureServiceClient(ApplicationConfiguration.Current.AdaptersClientConfiguration.SmsAdapter);

            return adapterClient;
        }

        public static APILogic.SmsAdapterService.AdapterStatus SetAdapaterConfiguration(ServiceClient client, SmsAdapterProfile adapter)
        {
            var configDict = adapter.Settings.ToDictionary(k => k.Key, v => v.Value);
            var settingsString = string.Concat(configDict.Select(kv => kv.Key + kv.Value));
            var signature = GenerateSignature(adapter.SharedSecret, adapter.Id, adapter.GroupId, settingsString);
            var configArray = configDict.Select(x => new KeyValue { Key = x.Key, Value = x.Value }).ToArray();
            _Logger.Debug($"Sms Adapter [{adapter.Id}] returned with no configuration. " +
                $"sending configuration: [{string.Concat(configDict.Select(kv => string.Format("[{0}|{1}], ", kv.Key, kv.Value)))}]");
            return client.SetConfigurationAsync((int)adapter.Id.Value, adapter.GroupId, configArray, signature).ExecuteAndWait();
        }

        private static string GenerateSignature(string secret, params object[] values)
        {
            var signatureStr = string.Concat(values);
            var signatureSHA1 = EncryptUtils.HashSHA1(signatureStr);
            var signatureAES = EncryptUtils.AesEncrypt(secret, signatureSHA1);
            var signature = Convert.ToBase64String(signatureAES);
            return signature;
        }
    }
}
