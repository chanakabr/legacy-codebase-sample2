using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using System;
using System.Reflection;
using KLogMonitor;
using System.Net.Http;
using System.Configuration;

namespace ApiLogic.Notification
{
    public class IotManager : ICrudHandler<Iot, long>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<IotManager> lazy = new Lazy<IotManager>(() => new IotManager());
        private const string REGISTER_PATH = "/api/IOT/RegisterDevice";
        private const string GET_CLIENT_PATH = "/api/IOT/Configuration/List?";
        public const string ADD_TO_SHADOW = "/api/IOT/AddToShadow";
        public const string PUBLISH = "/api/IOT/Publish";
        public const string DELETE_DEVICE = "/api/IOT/DeleteDevice";

        public static IotManager Instance { get { return lazy.Value; } }

        private IotManager() { }

        public GenericResponse<Iot> Register(ContextData contextData)
        {
            var response = new GenericResponse<Iot>();
            var groupId = contextData.GroupId;
            var udid = contextData.Udid;
            try
            {
                var partnerSettings = Core.Notification.NotificationCache.Instance().GetPartnerNotificationSettings(groupId);

                if (!IsIotAllowed(partnerSettings))
                {
                    log.Error($"Error while getting PartnerNotificationSettings for group: {groupId}.");
                    return response;
                }

                var _request = new { GroupId = groupId.ToString(), Udid = udid };
                var url = $"{partnerSettings.settings.IotAdapterUrl}{REGISTER_PATH}";

                var msResponse = Core.Notification.Adapters.NotificationAdapter.SendHttpRequest<Iot>
                    (url, Newtonsoft.Json.JsonConvert.SerializeObject(_request), HttpMethod.Post);

                if (msResponse == null)
                {
                    log.Error($"Error while registering udid: {udid} for group: {groupId}.");
                    return response;
                }

                var saved = SaveRegisteredDevice(groupId, udid, msResponse);

                response.SetStatus(saved ? Status.Ok : Status.Error);
                response.Object = msResponse;
            }
            catch (ConfigurationErrorsException)
            {
                log.Error($"No configurations for group: {groupId}.");
            }
            catch (Exception ex)
            {
                log.Error($"Register failed, error: {ex.Message}");
            }

            return response;
        }

        private static bool IsIotAllowed(ApiObjects.Notification.NotificationPartnerSettingsResponse partnerSettings)
        {
            return partnerSettings != null && partnerSettings.settings != null &&
                partnerSettings.settings.IsIotEnabled != null && partnerSettings.settings.IsIotEnabled.Value == true
                && !string.IsNullOrEmpty(partnerSettings.settings.IotAdapterUrl);
        }

        private bool SaveRegisteredDevice(int groupId, string udid, Iot msResponse)
        {
            msResponse.Udid = msResponse.Udid ?? udid;
            msResponse.GroupId = msResponse.GroupId ?? groupId.ToString();

            if (!DAL.NotificationDal.SaveRegisteredDevice(msResponse))
            {
                log.ErrorFormat($"Error while saving Iot device. Iot response: {Newtonsoft.Json.JsonConvert.SerializeObject(msResponse)}");
                return false;
            }
            return true;
        }

        public GenericResponse<IotClientConfiguration> GetIotClientConfiguration(ContextData contextData, bool isClient = true)
        {
            var response = new GenericResponse<IotClientConfiguration>();
            var groupId = contextData.GroupId;
            try
            {
                var partnerSettings = Core.Notification.NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
                if (!IsIotAllowed(partnerSettings))
                {
                    log.Error($"Error while getting PartnerNotificationSettings for group: {groupId}.");
                    return response;
                }

                var url = $"{partnerSettings.settings.IotAdapterUrl}{GET_CLIENT_PATH}groupId={groupId}&forClient={isClient}";

                var msResponse = Core.Notification.Adapters.NotificationAdapter.SendHttpRequest<IotClientConfiguration>
                    (url, Newtonsoft.Json.JsonConvert.SerializeObject(string.Empty), HttpMethod.Get);

                if (msResponse == null)
                {
                    log.Error($"Error while getting configurations for group: {groupId}.");
                    return response;
                }

                response.SetStatus(Status.Ok);
                response.Object = msResponse;
            }
            catch (ConfigurationErrorsException)
            {
                log.Error($"No configurations for group: {groupId}.");
            }
            catch (Exception ex)
            {
                log.Error($"GetIotClientConfiguration failed, error: {ex.Message}");
            }

            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<Iot> Get(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }
    }
}
