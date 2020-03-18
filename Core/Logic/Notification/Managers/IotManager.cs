using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using System;
using System.Reflection;
using KLogMonitor;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiLogic.Notification
{
    public class IotManager : ICrudHandler<Iot, long>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<IotManager> lazy = new Lazy<IotManager>(() => new IotManager());
        public static IotManager Instance { get { return lazy.Value; } }

        private IotManager() { }

        public GenericResponse<Iot> Register(ContextData contextData)
        {
            var response = new GenericResponse<Iot>();
            var groupId = contextData.GroupId;
            var udid = contextData.Udid;

            var partnerSettings = Core.Notification.NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            if (partnerSettings == null || partnerSettings.settings == null || string.IsNullOrEmpty(partnerSettings.settings.IotAdapterUrl))
            {
                log.Error($"Error while getting PartnerNotificationSettings for group: {groupId}.");
                return response;
            }

            var _request = new { GroupId = groupId.ToString(), Udid = udid };
            var url = $"{partnerSettings.settings.IotAdapterUrl}/api/IOT/RegisterDevice";

            var msResponse = Core.Notification.Adapters.NotificationAdapter.SendHttpRequest<Iot>
                (url, Newtonsoft.Json.JsonConvert.SerializeObject(_request), HttpMethod.Post);

            if (msResponse == null)
            {
                log.Error($"Error while registering udid: {udid} for group: {groupId}.");
                return response;
            }

            Task.Run(() => SaveRegisteredDevice(groupId, udid, msResponse));

            response.SetStatus(Status.Ok);
            response.Object = msResponse;

            return response;
        }

        private async Task SaveRegisteredDevice(int groupId, string udid, Iot msResponse)
        {
            msResponse.Udid = msResponse.Udid ?? udid;
            msResponse.GroupId = msResponse.GroupId ?? groupId.ToString();

            if (!DAL.DomainDal.SaveRegisteredDevice(msResponse))
            {
                log.ErrorFormat($"Error while saving Iot device. Iot response: {Newtonsoft.Json.JsonConvert.SerializeObject(msResponse)}");
            }

            if (DAL.DomainDal.GetRegisteredDevice(groupId.ToString(), udid) == null)
            {
                log.ErrorFormat($"Error while retrieving Iot device. Iot response: {Newtonsoft.Json.JsonConvert.SerializeObject(msResponse)}");
            }
        }

        public GenericResponse<IotClientConfiguration> GetIotClientConfiguration(ContextData contextData, bool isClient = true)
        {
            var response = new GenericResponse<IotClientConfiguration>();
            var groupId = contextData.GroupId;
            //var udid = contextData.Udid;

            var partnerSettings = Core.Notification.NotificationCache.Instance().GetPartnerNotificationSettings(groupId);
            if (partnerSettings == null || partnerSettings.settings == null || string.IsNullOrEmpty(partnerSettings.settings.IotAdapterUrl))
            {
                log.Error($"Error while getting PartnerNotificationSettings for group: {groupId}.");
                return response;
            }

            var url = $"{partnerSettings.settings.IotAdapterUrl}/api/IOT/Configuration/List?groupId={groupId}&forClient={isClient}";

            var msResponse = Core.Notification.Adapters.NotificationAdapter.SendHttpRequest<IotClientConfiguration>
                (url, Newtonsoft.Json.JsonConvert.SerializeObject(string.Empty), HttpMethod.Get);

            if (msResponse == null)
            {
                log.Error($"Error while getting configurations for group: {groupId}.");
                return response;
            }

            response.SetStatus(Status.Ok);
            response.Object = msResponse;

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
